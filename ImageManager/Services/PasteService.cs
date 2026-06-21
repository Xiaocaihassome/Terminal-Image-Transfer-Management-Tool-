using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ImageManager.Services;

public class PasteService : IPasteService
{
    private readonly IToastService _toastService;

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint CF_UNICODETEXT = 13;
    private const uint CF_HDROP = 15;

    public PasteService(IToastService toastService)
    {
        _toastService = toastService;
    }

    public async Task PasteImageAsync(string filePath, Window ownerWindow)
    {
        // 写剪贴板
        if (!WriteClipboard(filePath))
        {
            _toastService.Show("剪贴板写入失败", ToastType.Error);
            return;
        }

        await Task.Delay(100);

        // 最小化窗口 → 目标窗口自动获得前台焦点
        ownerWindow.WindowState = WindowState.Minimized;
        await Task.Delay(200);

        // 用 keybd_event 发 Ctrl+V（与 PowerShell 版本一致）
        SendCtrlV();
        await Task.Delay(300);

        // 恢复窗口
        ownerWindow.WindowState = WindowState.Normal;
        _toastService.Show("已粘贴路径", ToastType.Success);
    }

    private static void SendCtrlV()
    {
        keybd_event(0x11, 0, 0, IntPtr.Zero);       // Ctrl down
        keybd_event(0x56, 0, 0, IntPtr.Zero);       // V down
        keybd_event(0x56, 0, KEYEVENTF_KEYUP, IntPtr.Zero);  // V up
        keybd_event(0x11, 0, KEYEVENTF_KEYUP, IntPtr.Zero);  // Ctrl up
    }

    private static bool WriteClipboard(string filePath)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                try
                {
                    EmptyClipboard();
                    IntPtr hText = Marshal.StringToHGlobalUni(filePath + "\0");
                    SetClipboardData(CF_UNICODETEXT, hText);
                    IntPtr hDrop = CreateHDrop(filePath);
                    if (hDrop != IntPtr.Zero)
                        SetClipboardData(CF_HDROP, hDrop);
                    return true;
                }
                finally { CloseClipboard(); }
            }
            Thread.Sleep(50);
        }
        return false;
    }

    private static IntPtr CreateHDrop(string filePath)
    {
        int headerSize = 20;
        byte[] pathBytes = System.Text.Encoding.Unicode.GetBytes(filePath + "\0\0");
        int totalSize = headerSize + pathBytes.Length;
        IntPtr hMem = Marshal.AllocHGlobal(totalSize);
        try
        {
            for (int i = 0; i < totalSize; i++) Marshal.WriteByte(hMem, i, 0);
            Marshal.WriteInt32(hMem, 0, headerSize);
            Marshal.WriteInt32(hMem, 16, 1);
            Marshal.Copy(pathBytes, 0, hMem + headerSize, pathBytes.Length);
            return hMem;
        }
        catch { Marshal.FreeHGlobal(hMem); return IntPtr.Zero; }
    }
}
