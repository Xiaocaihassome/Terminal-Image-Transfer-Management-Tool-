using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ImageManager.Services;
using ImageManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ImageManager;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly IToastService _toastService;
    private readonly PasteService _pasteService;
    private readonly IConfigService _configService;
    private readonly IServiceProvider _serviceProvider;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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

    private const int SW_RESTORE = 9;
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;
    private const int SRCCOPY = 0x00CC0020;

    public MainWindow(MainViewModel viewModel, IToastService toastService, IPasteService pasteService, IConfigService configService, IServiceProvider serviceProvider)
    {
        _viewModel = viewModel;
        _toastService = toastService;
        _pasteService = (PasteService)pasteService;
        _configService = configService;
        _serviceProvider = serviceProvider;
        DataContext = viewModel;

        InitializeComponent();

        PreviewKeyDown += MainWindow_PreviewKeyDown;
        Closing += MainWindow_Closing;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _toastService.SetContainer(ToastContainer);
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

            var bmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
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
            // 捕获失败时使用纯色背景
            Background = new SolidColorBrush(Color.FromArgb(200, 240, 240, 240));
        }
    }

    #region 拖放

    private void DropZone_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (ThemeManager.Instance.Accent is SolidColorBrush accentBrush)
            {
                var c = accentBrush.Color;
                DropZone.Background = new SolidColorBrush(Color.FromArgb(30, c.R, c.G, c.B));
            }
            DropZone.BorderBrush = ThemeManager.Instance.Accent;
            e.Handled = true;
        }
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        DropZone.Background = Brushes.Transparent;
        DropZone.BorderBrush = ThemeManager.Instance.DropZoneBorder;
        e.Handled = true;
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        DropZone.Background = Brushes.Transparent;
        DropZone.BorderBrush = ThemeManager.Instance.DropZoneBorder;

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            await _viewModel.AddImagesAsync(files);
            if (files.Length > 0)
                _toastService.Show($"已添加 {files.Length} 张图片", ToastType.Success);
        }
        e.Handled = true;
    }

    #endregion

    #region 键盘

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
        {
            _viewModel.HandlePaste();
            e.Handled = true;
        }
        else if (e.Key == Key.Delete)
        {
            _viewModel.DeleteSelectedCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
        {
            _viewModel.IsAllSelected = !_viewModel.IsAllSelected;
            e.Handled = true;
        }
    }

    #endregion

    #region 标题栏

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal : WindowState.Maximized;
        }
        else
        {
            DragMove();
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void Close_Click(object sender, RoutedEventArgs e) =>
        Close();

    #endregion

    #region 按钮

    private async void PasteToWindow_Click(object sender, RoutedEventArgs e)
    {
        var selected = _viewModel.ImageItems.FirstOrDefault(i => i.IsSelected);
        if (selected == null)
        {
            _toastService.Show("请先选中要粘贴的图片", ToastType.Warning);
            return;
        }

        if (!File.Exists(selected.FilePath))
        {
            _toastService.Show("文件不存在", ToastType.Error);
            return;
        }

        try
        {
            await _pasteService.PasteImageAsync(selected.FilePath, this);
        }
        catch (Exception ex)
        {
            _toastService.Show($"粘贴失败：{ex.Message}", ToastType.Error);
        }
    }

    private void CopyPaths_Click(object sender, RoutedEventArgs e) =>
        _viewModel.CopySelectedPathsCommand.Execute(null);

    private void DeleteSelected_Click(object sender, RoutedEventArgs e) =>
        _viewModel.DeleteSelectedCommand.Execute(null);

    private void ClearAll_Click(object sender, RoutedEventArgs e) =>
        _viewModel.ClearAllCommand.Execute(null);

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var settingsVm = _serviceProvider.GetRequiredService<SettingsViewModel>();
        var settingsWindow = new SettingsWindow(settingsVm, _configService) { Owner = this };

        settingsWindow.Closed += (_, _) =>
        {
            _viewModel.SyncFromConfig(_configService);
            settingsVm.ApplyTheme(_configService.Theme, _configService.Transparency);
            ApplyFrostedGlass();
        };

        settingsWindow.ShowDialog();
    }

    #endregion

    #region 关闭

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) =>
        _viewModel.CleanupOnExit();

    #endregion
}
