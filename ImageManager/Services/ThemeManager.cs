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
        // 透明度越低，alpha 越小，毛玻璃模糊越明显
        double a = 0.08 + t * 0.35;
        CardBackground = MakeA(isDark ? "#1A1A1A" : "#F5F5F5", a);
        ListBackground = MakeA(isDark ? "#151515" : "#FFFFFF", a * 0.85);
        CardBorder = MakeA(isDark ? "#404040" : "#D0D0D0", a * 0.4);
        TextPrimary = Make(isDark ? "#FFFFFF" : "#1A1A1A");
        TextSecondary = Make(isDark ? "#AAAAAA" : "#666666");
        Accent = Make(isDark ? "#60CDFF" : "#0078D4");
        ButtonBg = MakeA(isDark ? "#383838" : "#E0E0E0", a);
        ButtonHover = MakeA(isDark ? "#484848" : "#D0D0D0", a);
        ButtonPressed = MakeA(isDark ? "#585858" : "#C0C0C0", a);
        DangerBg = MakeA(isDark ? "#502020" : "#FDE7E7", a);
        DangerFg = Make(isDark ? "#FF6B6B" : "#E81123");
        DangerHover = MakeA(isDark ? "#602525" : "#F5D0D0", a);
        DropZoneBorder = MakeA(isDark ? "#505050" : "#C0C0C0", a * 0.5);
        Separator = MakeA(isDark ? "#404040" : "#D0D0D0", a * 0.3);
        SettingsSectionBg = MakeA(isDark ? "#252525" : "#F0F0F0", a * 0.85);

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
        c.A = (byte)(opacity * 255);
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
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
