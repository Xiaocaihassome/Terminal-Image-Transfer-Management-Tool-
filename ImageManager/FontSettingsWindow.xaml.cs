using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageManager.Services;
using ImageManager.ViewModels;

namespace ImageManager;

public partial class FontSettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;
    private readonly IConfigService _configService;

    public FontSettingsWindow(SettingsViewModel viewModel, IConfigService configService)
    {
        _viewModel = viewModel;
        _configService = configService;
        DataContext = viewModel;
        InitializeComponent();
        try { Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/app.png", UriKind.Absolute)); }
        catch { }
        Loaded += FontSettingsWindow_Loaded;
    }

    private async void FontSettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(50);
        BackdropService.Apply(this, _configService?.BackgroundMode ?? "Glass");
        PreviewLarge.Text = $"{FindResource("AppTitle")} v{_viewModel.Version}";
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        var family = string.IsNullOrEmpty(_viewModel.SelectedFontFamily)
            ? new FontFamily("Microsoft YaHei UI")
            : new FontFamily(_viewModel.SelectedFontFamily);

        var weight = _viewModel.SelectedFontWeight switch
        {
            100 => FontWeights.Thin,
            200 => FontWeights.ExtraLight,
            300 => FontWeights.Light,
            400 => FontWeights.Normal,
            500 => FontWeights.Medium,
            600 => FontWeights.SemiBold,
            700 => FontWeights.Bold,
            800 => FontWeights.ExtraBold,
            900 => FontWeights.Black,
            _ => FontWeights.Normal
        };

        PreviewLarge.FontFamily = family;
        PreviewLarge.FontWeight = weight;
        PreviewMedium.FontFamily = family;
        PreviewMedium.FontWeight = weight;
        PreviewSmall.FontFamily = family;
        PreviewSmall.FontWeight = weight;
        PreviewChinese.FontFamily = family;
        PreviewChinese.FontWeight = weight;
        PreviewKorean.FontFamily = family;
        PreviewKorean.FontWeight = weight;
    }

    private void FontComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        UpdatePreview();
    }

    private void FontWeightSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        if (IsLoaded)
            UpdatePreview();
    }

    private void ApplyFont_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ApplyGlobalFont();
        _configService.Save();
        UpdatePreview();
    }

    private void ResetFont_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectedFontFamily = "";
        _viewModel.SelectedFontWeight = 400;
        _viewModel.ApplyGlobalFont();
        _configService.Save();
        UpdatePreview();
    }

    #region 窗口缩放

    private void Resize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement el) return;
        var direction = el.Name.Replace("Resize", "");

        double minWidth = 400, minHeight = 500;
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
        DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e) =>
        Close();

    #endregion
}
