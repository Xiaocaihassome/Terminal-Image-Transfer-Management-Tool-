using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Microsoft.Win32;

namespace ImageManager.Services;

public class ThemeManager : INotifyPropertyChanged
{
    public static ThemeManager Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // 所有主题 Brush
    public Brush CardBackground { get; private set; } = null!;
    public Brush ListBackground { get; private set; } = null!;
    public Brush CardBorder { get; private set; } = null!;
    public Brush TextPrimary { get; private set; } = null!;
    public Brush TextSecondary { get; private set; } = null!;
    public Brush Accent { get; private set; } = null!;
    public Brush ButtonBg { get; private set; } = null!;
    public Brush ButtonHover { get; private set; } = null!;
    public Brush ButtonPressed { get; private set; } = null!;
    public Brush DangerBg { get; private set; } = null!;
    public Brush DangerFg { get; private set; } = null!;
    public Brush DangerHover { get; private set; } = null!;
    public Brush DropZoneBorder { get; private set; } = null!;
    public Brush Separator { get; private set; } = null!;
    public Brush SettingsSectionBg { get; private set; } = null!;

    private ThemeManager()
    {
        Apply(false, 0.75);
    }

    public void ApplyTheme(string theme, double transparency = 0.75)
    {
        bool isDark = theme == "Dark" ||
                      (theme == "System" && IsSystemDark());
        Apply(isDark, transparency);
    }

    private void Apply(bool isDark, double t = 0.75)
    {
        // 非线性曲线：低透明度时通透，高透明度时接近实色
        double a = 0.08 + Math.Pow(t, 1.5) * 0.88;

        if (isDark)
        {
            // 深色主题：高透明度时使用更深的底色，低透明度时提亮避免发灰
            double lift = (1.0 - t) * 0.12; // 低透明度时轻微提亮
            CardBackground = MakeA(Lighten("#1A1A1A", lift), a);
            ListBackground = MakeA(Lighten("#121212", lift), a * 0.9);
            CardBorder = MakeA(Lighten("#3A3A3A", lift), Math.Min(1.0, a * 0.5 + 0.1));
            ButtonBg = MakeA(Lighten("#2A2A2A", lift), a);
            ButtonHover = MakeA(Lighten("#3A3A3A", lift), a);
            ButtonPressed = MakeA(Lighten("#4A4A4A", lift), a);
            DangerBg = MakeA("#502020", a);
            DangerHover = MakeA("#602525", a);
            DropZoneBorder = MakeA(Lighten("#454545", lift), Math.Min(1.0, a * 0.5 + 0.08));
            Separator = MakeA(Lighten("#353535", lift), Math.Min(1.0, a * 0.4 + 0.08));
            SettingsSectionBg = MakeA(Lighten("#1E1E1E", lift), a * 0.9);
        }
        else
        {
            CardBackground = MakeA("#F5F5F5", a);
            ListBackground = MakeA("#FFFFFF", a * 0.9);
            CardBorder = MakeA("#D0D0D0", a * 0.5);
            ButtonBg = MakeA("#E0E0E0", a);
            ButtonHover = MakeA("#D0D0D0", a);
            ButtonPressed = MakeA("#C0C0C0", a);
            DangerBg = MakeA("#FDE7E7", a);
            DangerHover = MakeA("#F5D0D0", a);
            DropZoneBorder = MakeA("#C0C0C0", a * 0.5);
            Separator = MakeA("#D0D0D0", a * 0.35);
            SettingsSectionBg = MakeA("#F0F0F0", a * 0.9);
        }

        TextPrimary = Make(isDark ? "#FFFFFF" : "#1A1A1A");
        TextSecondary = Make(isDark ? "#B0B0B0" : "#666666");
        Accent = Make(isDark ? "#60CDFF" : "#0078D4");
        DangerFg = Make(isDark ? "#FF6B6B" : "#E81123");

        // 通知所有属性变更
        OnChanged(nameof(CardBackground));
        OnChanged(nameof(ListBackground));
        OnChanged(nameof(CardBorder));
        OnChanged(nameof(TextPrimary));
        OnChanged(nameof(TextSecondary));
        OnChanged(nameof(Accent));
        OnChanged(nameof(ButtonBg));
        OnChanged(nameof(ButtonHover));
        OnChanged(nameof(ButtonPressed));
        OnChanged(nameof(DangerBg));
        OnChanged(nameof(DangerFg));
        OnChanged(nameof(DangerHover));
        OnChanged(nameof(DropZoneBorder));
        OnChanged(nameof(Separator));
        OnChanged(nameof(SettingsSectionBg));
    }

    private static Brush Make(string hex)
    {
        var c = (Color)ColorConverter.ConvertFromString(hex);
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }

    private static Brush MakeA(string hex, double opacity)
    {
        var c = (Color)ColorConverter.ConvertFromString(hex);
        c.A = (byte)(Math.Clamp(opacity, 0, 1) * 255);
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }

    /// <summary>将深色往白色方向提亮（用于低透明度时避免发灰）。</summary>
    private static string Lighten(string hex, double amount)
    {
        var c = (Color)ColorConverter.ConvertFromString(hex);
        int r = c.R + (int)((255 - c.R) * amount);
        int g = c.G + (int)((255 - c.G) * amount);
        int b = c.B + (int)((255 - c.B) * amount);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static bool IsSystemDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int i && i == 0;
        }
        catch { return false; }
    }
}
