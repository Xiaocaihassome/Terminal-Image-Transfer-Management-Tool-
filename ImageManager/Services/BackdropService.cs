using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace ImageManager.Services;

/// <summary>
/// 统一管理窗口背景效果：毛玻璃、液态玻璃、Win11 亚克力、纯色。
/// </summary>
public static class BackdropService
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;
    private const int SRCCOPY = 0x00CC0020;

    // ===== Win11 亚克力（SetWindowCompositionAttribute）=====
    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public int AccentState;
        public int AccentFlags;
        public uint GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public int Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    private const int WCA_ACCENT_POLICY = 19;
    private const int ACCENT_DISABLED = 0;
    private const int ACCENT_ENABLE_ACRYLICBLURBEHIND = 4;

    /// <summary>
    /// 根据模式应用背景。mode: Glass / Mica / None。
    /// </summary>
    public static void Apply(Window window, string mode)
    {
        // 清除可能残留的亚克力
        DisableAcrylic(window);

        switch (mode)
        {
            case "None":
                window.Background = new SolidColorBrush(Color.FromArgb(240, 240, 240, 240));
                break;

            case "Mica":
                // 在自绘透明窗口上使用亚克力模糊作为 Win11 系统材质效果
                window.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                EnableAcrylic(window);
                break;

            case "Glass":
            default:
                window.Background = CaptureBlurredDesktop(30);
                break;
        }
    }

    /// <summary>截取桌面并高斯模糊作为背景画刷。</summary>
    private static Brush CaptureBlurredDesktop(double blurRadius)
    {
        try
        {
            int w = GetSystemMetrics(SM_CXSCREEN);
            int h = GetSystemMetrics(SM_CYSCREEN);

            IntPtr hDesktop = GetDesktopWindow();
            IntPtr hDCDesktop = GetWindowDC(hDesktop);
            IntPtr hDCMem = CreateCompatibleDC(hDCDesktop);
            IntPtr hBitmap = CreateCompatibleBitmap(hDCDesktop, w, h);
            IntPtr hOld = SelectObject(hDCMem, hBitmap);

            BitBlt(hDCMem, 0, 0, w, h, hDCDesktop, 0, 0, SRCCOPY);

            SelectObject(hDCMem, hOld);
            DeleteDC(hDCMem);
            ReleaseDC(hDesktop, hDCDesktop);

            var bmp = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap);
            bmp.Freeze();

            var visualBrush = new VisualBrush(new Image { Source = bmp, Stretch = Stretch.None });
            visualBrush.Freeze();

            var blurBrush = new VisualBrush(new Border
            {
                Background = visualBrush,
                Effect = new BlurEffect { Radius = blurRadius, KernelType = KernelType.Gaussian }
            });
            blurBrush.Freeze();
            return blurBrush;
        }
        catch
        {
            return new SolidColorBrush(Color.FromArgb(200, 240, 240, 240));
        }
    }

    private static void EnableAcrylic(Window window)
    {
        try
        {
            var hwnd = new WindowInteropHelper(window).EnsureHandle();
            // 半透明底色（AABBGGRR），alpha 越低越通透
            var accent = new AccentPolicy
            {
                AccentState = ACCENT_ENABLE_ACRYLICBLURBEHIND,
                GradientColor = 0x10_FFFFFF
            };
            int size = Marshal.SizeOf(accent);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(accent, ptr, false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = WCA_ACCENT_POLICY,
                Data = ptr,
                SizeOfData = size
            };
            SetWindowCompositionAttribute(hwnd, ref data);
            Marshal.FreeHGlobal(ptr);
        }
        catch { }
    }

    private static void DisableAcrylic(Window window)
    {
        try
        {
            var helper = new WindowInteropHelper(window);
            if (helper.Handle == IntPtr.Zero) return; // 句柄未创建时无需清除
            var accent = new AccentPolicy { AccentState = ACCENT_DISABLED };
            int size = Marshal.SizeOf(accent);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(accent, ptr, false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = WCA_ACCENT_POLICY,
                Data = ptr,
                SizeOfData = size
            };
            SetWindowCompositionAttribute(helper.Handle, ref data);
            Marshal.FreeHGlobal(ptr);
        }
        catch { }
    }
}
