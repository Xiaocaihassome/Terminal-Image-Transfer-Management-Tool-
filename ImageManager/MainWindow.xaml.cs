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
    private readonly IFileService _fileService;
    private readonly DispatcherTimer _clipboardTimer;
    private string _lastClipboardHash = string.Empty;

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

    public MainWindow(MainViewModel viewModel, IToastService toastService, IPasteService pasteService, IConfigService configService, IServiceProvider serviceProvider, IFileService fileService)
    {
        _viewModel = viewModel;
        _toastService = toastService;
        _pasteService = (PasteService)pasteService;
        _configService = configService;
        _serviceProvider = serviceProvider;
        _fileService = fileService;
        DataContext = viewModel;

        InitializeComponent();

        // 加载窗口图标
        try { Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/app.png", UriKind.Absolute)); }
        catch { }

        PreviewKeyDown += MainWindow_PreviewKeyDown;
        Closing += MainWindow_Closing;
        Loaded += MainWindow_Loaded;

        // 初始化剪贴板监听定时器
        _clipboardTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _clipboardTimer.Tick += ClipboardTimer_Tick;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _toastService.SetContainer(ToastContainer);
        await Task.Delay(50);
        if (!_configService.DisableBlur)
            ApplyFrostedGlass();

        // 根据配置启动或停止剪贴板监听
        UpdateClipboardMonitor();
    }

    private void UpdateClipboardMonitor()
    {
        if (_configService.AutoDetectClipboard)
        {
            _clipboardTimer.Start();
        }
        else
        {
            _clipboardTimer.Stop();
        }
    }

    private async void ClipboardTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                if (image != null)
                {
                    // 生成简单的哈希来检测变化
                    var hash = $"{image.Width}_{image.Height}_{image.DpiX}_{image.DpiY}";
                    if (hash != _lastClipboardHash)
                    {
                        _lastClipboardHash = hash;
                        
                        // 保存图片到临时目录
                        var path = _fileService.SaveImageToTemp(image);
                        await _viewModel.AddImageAsync(path);
                        _toastService.Show("已自动保存剪贴板图片", ToastType.Success);
                    }
                }
            }
        }
        catch { }
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
            if (ThemeManager.Instance.Accent is SolidColorBrush accent)
                DropZone.BorderBrush = accent;
        }
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        DropZone.BorderBrush = (Brush)FindResource("DropZoneBorder");
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        DropZone.BorderBrush = (Brush)FindResource("DropZoneBorder");
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            await _viewModel.AddImagesAsync(files);
        }
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

    #region 窗口缩放

    private void Resize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement el) return;
        var direction = el.Name.Replace("Resize", "");

        double minWidth = MinWidth, minHeight = MinHeight;
        double maxWidth = SystemParameters.PrimaryScreenWidth;
        double maxHeight = SystemParameters.PrimaryScreenHeight;

        el.CaptureMouse();
        var anchor = PointToScreen(e.GetPosition(this));
        double origLeft = Left, origTop = Top, origW = Width, origH = Height;

        void OnMove(object s, MouseEventArgs ev)
        {
            var cur = PointToScreenMouse();
            double dx = cur.X - anchor.X, dy = cur.Y - anchor.Y;

            if (direction.Contains("W"))
            {
                double w = Math.Clamp(origW - dx, minWidth, maxWidth);
                Left = origLeft + origW - w;
                Width = w;
            }
            if (direction.Contains("E"))
                Width = Math.Clamp(origW + dx, minWidth, maxWidth);
            if (direction.Contains("N"))
            {
                double h = Math.Clamp(origH - dy, minHeight, maxHeight);
                Top = origTop + origH - h;
                Height = h;
            }
            if (direction.Contains("S"))
                Height = Math.Clamp(origH + dy, minHeight, maxHeight);
        }

        void OnUp(object s, MouseButtonEventArgs ev)
        {
            el.ReleaseMouseCapture();
            el.MouseMove -= OnMove;
            el.MouseLeftButtonUp -= OnUp;
        }

        el.MouseMove += OnMove;
        el.MouseLeftButtonUp += OnUp;
    }

    private Point PointToScreenMouse()
    {
        var p = Mouse.GetPosition(this);
        return new Point(p.X + Left, p.Y + Top);
    }

    #endregion

    #region 标题栏

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
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

    private void UpdateBanner_Click(object sender, RoutedEventArgs e)
    {
        Settings_Click(sender, e);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var settingsVm = _serviceProvider.GetRequiredService<SettingsViewModel>();
        var settingsWindow = new SettingsWindow(settingsVm, _configService) { Owner = this };

        settingsWindow.Closed += (_, _) =>
        {
            _viewModel.SyncFromConfig(_configService);
            settingsVm.ApplyTheme(_configService.Theme, _configService.Transparency);
            if (_configService.DisableBlur)
                Background = new SolidColorBrush(Color.FromArgb(240, 240, 240, 240));
            else
                ApplyFrostedGlass();
            
            // 更新剪贴板监听状态
            UpdateClipboardMonitor();
        };

        settingsWindow.ShowDialog();
    }

    #endregion

    #region 关闭

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _clipboardTimer.Stop();
        _viewModel.CleanupOnExit();
    }

    #endregion
}
