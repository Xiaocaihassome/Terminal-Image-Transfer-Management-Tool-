using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using ImageManager.Services;
using ImageManager.ViewModels;

namespace ImageManager;

public partial class SettingsWindow : Window
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

    public IConfigService ConfigService { get; }

    public SettingsWindow()
    {
        InitializeComponent();
        Loaded += SettingsWindow_Loaded;
    }

    public SettingsWindow(SettingsViewModel viewModel, IConfigService configService) : this()
    {
        DataContext = viewModel;
        ConfigService = configService;
    }

    private async void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(50);
        ApplyFrostedGlass();
    }

    private void ApplyFrostedGlass()
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
                Effect = new BlurEffect { Radius = 30, KernelType = KernelType.Gaussian }
            });
            blurBrush.Freeze();

            Background = blurBrush;
        }
        catch
        {
            Background = new SolidColorBrush(Color.FromArgb(200, 240, 240, 240));
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
