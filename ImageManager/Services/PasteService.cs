using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageManager.Services;

public class PasteService : IPasteService
{
    private readonly IToastService _toastService;
    private readonly Dispatcher _dispatcher;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const ushort VK_CONTROL = 0x11;
    private const ushort VK_V = 0x56;
    private const int SW_RESTORE = 9;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr extraInfo;
    }

    public PasteService(IToastService toastService)
    {
        _toastService = toastService;
        _dispatcher = Application.Current.Dispatcher;
    }

    public async Task PasteImageAsync(string filePath, Window ownerWindow)
    {
        // 2秒倒计时
        for (int i = 2; i > 0; i--)
        {
            _toastService.Show($"请切换到目标窗口，{i}秒后粘贴路径...", ToastType.Info);
            await Task.Delay(1000);
        }

        // 写入剪贴板 + 发送 Ctrl+V（同一 dispatcher 调用，确保时序）
        await _dispatcher.InvokeAsync(() =>
        {
            var hwnd = new WindowInteropHelper(ownerWindow).Handle;
            if (GetForegroundWindow() == hwnd)
            {
                _toastService.Show("请先切换到目标窗口", ToastType.Warning);
                return;
            }

            // 写入多种格式，确保兼容性
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.UnicodeText, filePath, true);
            dataObject.SetData(DataFormats.Text, filePath, true);
            dataObject.SetData(DataFormats.FileDrop, new string[] { filePath }, true);
            Clipboard.SetDataObject(dataObject, true);

            Thread.Sleep(100);
            SendCtrlV();
            _toastService.Show("已粘贴路径", ToastType.Success);
        });
    }

    private void SendCtrlV()
    {
        var inputs = new INPUT[4];

        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].U.ki.wVk = VK_CONTROL;

        inputs[1].type = INPUT_KEYBOARD;
        inputs[1].U.ki.wVk = VK_V;

        inputs[2].type = INPUT_KEYBOARD;
        inputs[2].U.ki.wVk = VK_V;
        inputs[2].U.ki.dwFlags = KEYEVENTF_KEYUP;

        inputs[3].type = INPUT_KEYBOARD;
        inputs[3].U.ki.wVk = VK_CONTROL;
        inputs[3].U.ki.dwFlags = KEYEVENTF_KEYUP;

        SendInput(4, inputs, Marshal.SizeOf<INPUT>());
    }
}
