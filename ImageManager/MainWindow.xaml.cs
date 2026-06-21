using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
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

    public MainWindow(MainViewModel viewModel, IToastService toastService, IPasteService pasteService, IConfigService configService, IServiceProvider serviceProvider)
    {
        _viewModel = viewModel;
        _toastService = toastService;
        _pasteService = (PasteService)pasteService;
        _configService = configService;
        _serviceProvider = serviceProvider;
        DataContext = viewModel;

        InitializeComponent();

        try { Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/app.png", UriKind.Absolute)); }
        catch { }

        PreviewKeyDown += MainWindow_PreviewKeyDown;
        Closing += MainWindow_Closing;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _toastService.SetContainer(ToastContainer);
        await Task.Delay(50);
        BackdropService.Apply(this, _configService.BackgroundMode);

        // 轮询检查 HasUpdate，更新 banner 可见性
        _ = PollUpdateBanner();
    }

    private async Task PollUpdateBanner()
    {
        // 等后台检查完成
        for (int i = 0; i < 20; i++)
        {
            if (_viewModel.HasUpdate)
            {
                UpdateBanner.Visibility = Visibility.Visible;
                return;
            }
            await Task.Delay(500);
        }
    }

    #region 拖放

    private void DropZone_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (ThemeManager.Instance.Accent is System.Windows.Media.SolidColorBrush accentBrush)
            {
                var c = accentBrush.Color;
                DropZone.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(30, c.R, c.G, c.B));
            }
            DropZone.BorderBrush = ThemeManager.Instance.Accent;
            e.Handled = true;
        }
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        DropZone.Background = System.Windows.Media.Brushes.Transparent;
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
        DropZone.Background = System.Windows.Media.Brushes.Transparent;
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
            var p = Mouse.GetPosition(this);
            var cur = new Point(p.X + Left, p.Y + Top);
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

    #endregion

    #region 标题栏

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
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
            BackdropService.Apply(this, _configService.BackgroundMode);
        };

        settingsWindow.ShowDialog();
    }

    #endregion

    #region 关闭

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) =>
        _viewModel.CleanupOnExit();

    #endregion

    private void UpdateBanner_Click(object sender, RoutedEventArgs e) =>
        Settings_Click(sender, e);
}
