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
        await PasteImagesAsync(new[] { filePath }, ownerWindow);
    }

    public async Task PasteImagesAsync(IEnumerable<string> filePaths, Window ownerWindow)
    {
        var paths = filePaths.ToArray();
        if (paths.Length == 0) return;

        // 写剪贴板
        if (!WriteClipboard(paths))
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
        _toastService.Show($"已粘贴 {paths.Length} 个文件路径", ToastType.Success);
    }

    private static void SendCtrlV()
    {
        keybd_event(0x11, 0, 0, IntPtr.Zero);       // Ctrl down
        keybd_event(0x56, 0, 0, IntPtr.Zero);       // V down
        keybd_event(0x56, 0, KEYEVENTF_KEYUP, IntPtr.Zero);  // V up
        keybd_event(0x11, 0, KEYEVENTF_KEYUP, IntPtr.Zero);  // Ctrl up
    }

    private static bool WriteClipboard(string[] filePaths)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                try
                {
                    EmptyClipboard();

                    // CF_UNICODETEXT: 所有路径用换行连接
                    var text = string.Join("\n", filePaths);
                    IntPtr hText = Marshal.StringToHGlobalUni(text + "\0");
                    SetClipboardData(CF_UNICODETEXT, hText);

                    // CF_HDROP: 支持多文件
                    IntPtr hDrop = CreateHDrop(filePaths);
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

    private static IntPtr CreateHDrop(string[] filePaths)
    {
        // HDrop 结构:
        // [0-3]   header size (uint32)
        // [4-15]  reserved (全0，包含 point 结构)
        // [16-19] 文件数量 (uint32)
        // [20...] 每个文件路径: null-terminated Unicode，末尾双 null 结束
        int headerSize = 20;
        int fileCount = filePaths.Length;

        // 计算总路径字节数
        int pathsBytes = 0;
        foreach (var path in filePaths)
            pathsBytes += System.Text.Encoding.Unicode.GetByteCount(path + "\0");
        pathsBytes += 2; // 末尾双 null

        int totalSize = headerSize + pathsBytes;
        IntPtr hMem = Marshal.AllocHGlobal(totalSize);
        try
        {
            // 清零
            for (int i = 0; i < totalSize; i++) Marshal.WriteByte(hMem, i, 0);

            Marshal.WriteInt32(hMem, 0, headerSize);
            Marshal.WriteInt32(hMem, 16, fileCount);

            int offset = headerSize;
            foreach (var path in filePaths)
            {
                byte[] pathBytes = System.Text.Encoding.Unicode.GetBytes(path + "\0");
                Marshal.Copy(pathBytes, 0, hMem + offset, pathBytes.Length);
                offset += pathBytes.Length;
            }
            return hMem;
        }
        catch { Marshal.FreeHGlobal(hMem); return IntPtr.Zero; }
    }
}
