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
    private const uint PW_RENDERFULLCONTENT = 0x00000002;

    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hwnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    /// <summary>
    /// 根据模式应用背景。mode: Glass / Mica / None。
    /// 返回 true 表示效果成功应用，false 表示降级。
    /// </summary>
    public static bool Apply(Window window, string mode)
    {
        // 清除可能残留的亚克力
        DisableAcrylic(window);

        switch (mode)
        {
            case "None":
                window.Background = new SolidColorBrush(Color.FromArgb(240, 240, 240, 240));
                return true;

            case "Mica":
                // 在自绘透明窗口上使用亚克力模糊作为 Win11 系统材质效果
                window.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                return EnableAcrylic(window);

            case "Glass":
            default:
                window.Background = CaptureBlurredDesktop(30);
                return true;
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

    private static bool EnableAcrylic(Window window)
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

            // SetWindowCompositionAttribute 在不支持的系统上仍返回 1，
            // 需通过截取窗口像素检查 alpha 通道判断效果是否真的生效
            return VerifyAcrylicRendering(hwnd);
        }
        catch { return false; }
    }

    /// <summary>
    /// 截取窗口渲染结果，检查像素是否具有透明度。
    /// 亚克力效果会使窗口背景半透明（alpha &lt; 255），
    /// 如果所有像素均不透明则说明效果未生效。
    /// </summary>
    private static bool VerifyAcrylicRendering(IntPtr hwnd)
    {
        try
        {
            Thread.Sleep(80); // 等待 DWM 合成完成

            GetClientRect(hwnd, out RECT rect);
            int w = rect.Right - rect.Left;
            int h = rect.Bottom - rect.Top;
            if (w <= 0 || h <= 0) return true; // 无法检测时默认成功

            using var bmp = new System.Drawing.Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var hdcBmp = System.Drawing.Graphics.FromImage(bmp);
            IntPtr hdc = hdcBmp.GetHdc();

            bool ok = PrintWindow(hwnd, hdc, PW_RENDERFULLCONTENT);
            hdcBmp.ReleaseHdc();

            if (!ok) return true; // PrintWindow 失败时无法判断，默认成功

            // 采样中心 40x40 区域的 alpha 通道
            int cx = w / 2, cy = h / 2;
            int sampleSize = Math.Min(40, Math.Min(w, h) / 2);
            int opaqueCount = 0, total = 0;
            for (int dy = -sampleSize; dy < sampleSize; dy += 4)
            {
                for (int dx = -sampleSize; dx < sampleSize; dx += 4)
                {
                    int px = cx + dx, py = cy + dy;
                    if (px >= 0 && px < w && py >= 0 && py < h)
                    {
                        total++;
                        if (bmp.GetPixel(px, py).A == 255) opaqueCount++;
                    }
                }
            }

            // 全部不透明 → 亚克力效果未生效
            return total == 0 || opaqueCount < total;
        }
        catch { return true; }
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
