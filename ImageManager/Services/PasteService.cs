using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ImageManager.Services;

public class PasteService : IPasteService
{
    private readonly IToastService _toastService;
    private readonly IConfigService _configService;

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

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint CF_UNICODETEXT = 13;
    private const uint CF_HDROP = 15;
    private const int SW_RESTORE = 9;

    public PasteService(IToastService toastService, IConfigService configService)
    {
        _toastService = toastService;
        _configService = configService;
    }

    public async Task PasteImageAsync(string filePath, Window ownerWindow)
    {
        await PasteImagesAsync(new[] { filePath }, ownerWindow);
    }

    public async Task PasteImagesAsync(IEnumerable<string> filePaths, Window ownerWindow)
    {
        var paths = filePaths.ToArray();
        if (paths.Length == 0) return;

        // 记录目标窗口（粘贴前的前台窗口）
        IntPtr targetHwnd = GetForegroundWindow();
        IntPtr ownerHwnd = new WindowInteropHelper(ownerWindow).Handle;

        // 如果当前前台窗口是本窗口，则不记录
        if (targetHwnd == ownerHwnd)
            targetHwnd = IntPtr.Zero;

        // 写剪贴板
        if (!WriteClipboard(paths))
        {
            _toastService.Show("剪贴板写入失败", ToastType.Error);
            return;
        }

        await Task.Delay(100);

        // 最小化窗口
        ownerWindow.WindowState = WindowState.Minimized;
        await Task.Delay(200);

        // 用 keybd_event 发 Ctrl+V
        SendCtrlV();
        await Task.Delay(300);

        // 根据配置决定是否回到目标窗口
        if (_configService.AutoReturnToTarget && targetHwnd != IntPtr.Zero)
        {
            ShowWindow(targetHwnd, SW_RESTORE);
            SetForegroundWindow(targetHwnd);
        }
        else
        {
            // 恢复本窗口
            ownerWindow.WindowState = WindowState.Normal;
        }

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

                    var text = string.Join("\n", filePaths);
                    IntPtr hText = Marshal.StringToHGlobalUni(text + "\0");
                    SetClipboardData(CF_UNICODETEXT, hText);

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
        int headerSize = 20;
        int fileCount = filePaths.Length;

        int pathsBytes = 0;
        foreach (var path in filePaths)
            pathsBytes += System.Text.Encoding.Unicode.GetByteCount(path + "\0");
        pathsBytes += 2;

        int totalSize = headerSize + pathsBytes;
        IntPtr hMem = Marshal.AllocHGlobal(totalSize);
        try
        {
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
