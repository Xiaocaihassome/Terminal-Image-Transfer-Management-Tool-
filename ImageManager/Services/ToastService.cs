using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ImageManager.Services;

public class ToastService : IToastService
{
    private Panel? _container;
    private readonly Dispatcher _dispatcher;

    public ToastService()
    {
        _dispatcher = Application.Current.Dispatcher;
    }

    public void SetContainer(Panel container)
    {
        _container = container;
    }

    public void Show(string message, ToastType type = ToastType.Info)
    {
        _dispatcher.Invoke(() => ShowInternal(message, type));
    }

    private void ShowInternal(string message, ToastType type)
    {
        if (_container == null) return;

        var bgColor = type switch
        {
            ToastType.Success => (Color)ColorConverter.ConvertFromString("#34C759"),
            ToastType.Warning => (Color)ColorConverter.ConvertFromString("#FF9500"),
            ToastType.Error => (Color)ColorConverter.ConvertFromString("#FF3B30"),
            _ => (Color)ColorConverter.ConvertFromString("#007AFF"),
        };

        var border = new Border
        {
            CornerRadius = new CornerRadius(12),
            Background = new SolidColorBrush(bgColor),
            Padding = new Thickness(16, 10, 16, 10),
            Margin = new Thickness(0, 0, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 20,
                Opacity = 0.3,
                ShadowDepth = 4
            },
            Child = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 400
            }
        };

        // 入场动画
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2));
        var slideIn = new ThicknessAnimation(new Thickness(0, 20, 0, 0), new Thickness(0), TimeSpan.FromSeconds(0.3))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        border.Opacity = 0;
        border.Margin = new Thickness(0, 20, 0, 0);

        _container.Children.Add(border);

        border.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        border.BeginAnimation(FrameworkElement.MarginProperty, slideIn);

        // 3秒后淡出消失
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            fadeOut.Completed += (s2, e2) => _container.Children.Remove(border);
            border.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        };
        timer.Start();
    }
}
