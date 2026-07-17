using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ImageManager.Services;
using ImageManager.ViewModels;

namespace ImageManager;

public partial class SettingsWindow : Window
{
    public IConfigService ConfigService { get; }
    private bool _isLoaded;

    public SettingsWindow()
    {
        InitializeComponent();
        try { Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/app.png", UriKind.Absolute)); }
        catch { }
        Loaded += SettingsWindow_Loaded;
    }

    public SettingsWindow(SettingsViewModel viewModel, IConfigService configService) : this()
    {
        DataContext = viewModel;
        ConfigService = configService;
        viewModel.RefreshBackdrop = () =>
            BackdropService.Apply(this, configService.BackgroundMode);
        viewModel.TriggerGlow = (color) => PlayGlow(AboutCard, color);
    }

    private async void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(50);
        BackdropService.Apply(this, ConfigService?.BackgroundMode ?? "Glass");
        SubscribeGlowEvents();
        _isLoaded = true;
    }

    private void SubscribeGlowEvents()
    {
        // 给每个卡片里的所有交互元素统一加发光
        SubscribeCard(AppearanceCard);
        SubscribeCard(LanguageCard);
        SubscribeCard(StorageCard, Color.FromRgb(0xE8, 0x11, 0x23));
        SubscribeCard(PasteCard);
        SubscribeCard(GeneralCard);
        SubscribeCard(AboutCard);

        // 字体设置卡片（如果有的话）
        var fontCard = FindName("FontCard") as Border;
        if (fontCard != null) SubscribeCard(fontCard);
    }

    private void SubscribeCard(Border card, Color? glowColor = null)
    {
        if (card == null) return;

        // RadioButton
        foreach (var rb in FindVisualChildren<RadioButton>(card))
            rb.Checked += (_, _) => PlayGlow(card, glowColor);

        // CheckBox
        foreach (var cb in FindVisualChildren<CheckBox>(card))
        {
            cb.Checked += (_, _) => PlayGlow(card, glowColor);
            cb.Unchecked += (_, _) => PlayGlow(card, glowColor);
        }

        // Button
        foreach (var btn in FindVisualChildren<System.Windows.Controls.Button>(card))
            btn.Click += (_, _) => PlayGlow(card, glowColor);
    }

    private static IEnumerable<T> FindVisualChildren<T>(System.Windows.DependencyObject dep) where T : System.Windows.DependencyObject
    {
        if (dep == null) yield break;
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(dep); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(dep, i);
            if (child is T typed) yield return typed;
            foreach (var c in FindVisualChildren<T>(child)) yield return c;
        }
    }

    private void PrivacyCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoaded && BehaviorCard != null) PlayGlow(BehaviorCard, Color.FromRgb(0x22, 0x22, 0x22));
    }

    private void AutoDetectClipboardCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoaded && BehaviorCard != null) PlayGlow(BehaviorCard);
    }

    private void AutoReturnToTargetCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoaded && BehaviorCard != null) PlayGlow(BehaviorCard);
    }

    private void AlwaysOnTopCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoaded && BehaviorCard != null) PlayGlow(BehaviorCard);
    }

    private void BehaviorDangerCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoaded && BehaviorCard != null) PlayGlow(BehaviorCard, Color.FromRgb(0xE8, 0x11, 0x23));
    }

    private void SkipUpdateReminder_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoaded && sender is System.Windows.Controls.CheckBox cb && BehaviorCard != null)
        {
            if (cb.IsChecked == true)
                PlayGlow(BehaviorCard, Color.FromRgb(0xE8, 0x11, 0x23), 2000);
            else
                PlayGlow(BehaviorCard, Color.FromRgb(0x2E, 0xA0, 0x43));
        }
    }

    private void ThemeRadio_Checked(object sender, RoutedEventArgs e) { if (AppearanceCard != null) PlayGlow(AppearanceCard); }
    private void LanguageRadio_Checked(object sender, RoutedEventArgs e) { if (LanguageCard != null) PlayGlow(LanguageCard); }
    private void StorageButton_Click(object sender, RoutedEventArgs e) { if (StorageCard != null) PlayGlow(StorageCard); }
    private void StorageCheckBox_Changed(object sender, RoutedEventArgs e) { if (StorageCard != null) PlayGlow(StorageCard); }
    private void UpdateButton_Click(object sender, RoutedEventArgs e) { if (AboutCard != null) PlayGlow(AboutCard); }

    private void PlayGlow(Border card, Color? glowColor = null, int durationMs = 1200)
    {
        if (card == null) return;
        try
        {
            Color accentColor;
            if (glowColor.HasValue)
            {
                accentColor = glowColor.Value;
            }
            else if (ThemeManager.Instance.Accent is SolidColorBrush ab)
            {
                accentColor = ab.Color;
            }
            else
            {
                accentColor = Color.FromRgb(96, 205, 255);
            }

            // 保存原始 binding
            var origBinding = BindingOperations.GetBinding(card, Border.BorderBrushProperty);

            var glowBrush = new SolidColorBrush(accentColor);
            card.BorderBrush = glowBrush;

            var anim = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            glowBrush.BeginAnimation(SolidColorBrush.OpacityProperty, anim);

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(durationMs)
            };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                fadeOut.Completed += (_, _) =>
                {
                    if (origBinding != null)
                        BindingOperations.SetBinding(card, Border.BorderBrushProperty, origBinding);
                    else
                        card.ClearValue(Border.BorderBrushProperty);
                };
                glowBrush.BeginAnimation(SolidColorBrush.OpacityProperty, fadeOut);
            };
            timer.Start();
        }
        catch { }
    }

    #region 窗口缩放

    private void Resize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement el) return;
        var direction = el.Name.Replace("Resize", "");

        double minWidth = 350, minHeight = 400;
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

    private void OpenRepo_Click(object sender, MouseButtonEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-",
            UseShellExecute = true
        });
    }

    private void OpenWebsite_Click(object sender, MouseButtonEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://imagemanager-6gs.pages.dev/",
            UseShellExecute = true
        });
    }

    private void OpenLogs_Click(object sender, RoutedEventArgs e)
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ImageManager", "Logs");
        Directory.CreateDirectory(logDir);
        System.Diagnostics.Process.Start("explorer.exe", logDir);
    }

    private void OpenFontSettings_Click(object sender, MouseButtonEventArgs e)
    {
        var vm = DataContext as SettingsViewModel;
        if (vm == null) return;
        var fontWindow = new FontSettingsWindow(vm, ConfigService) { Owner = this };
        fontWindow.ShowDialog();
    }
}
