using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageManager.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;
        bool invert = parameter is string s && s == "Invert";
        if (invert) flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Visibility v && v == Visibility.Visible;
}

public class EqualityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string s && int.TryParse(s, out int target))
            return (value is int i && i == target);
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class ThemeToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b && b && parameter is string s)
            return s;
        return DependencyProperty.UnsetValue;
    }
}

/// <summary>隐私模式下遮罩文件路径，只显示文件名。</summary>
public class PrivacyPathConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        string path = values.Length > 0 ? values[0] as string ?? "" : "";
        if (values.Length < 2) return path;

        bool privacyMode = values[1] is bool pm && pm;
        if (!privacyMode) return path;

        // 只显示文件名，路径用 * 遮罩
        var dir = System.IO.Path.GetDirectoryName(path);
        var name = System.IO.Path.GetFileName(path);
        if (string.IsNullOrEmpty(dir)) return name ?? path;
        return name + " (" + new string('*', Math.Min(dir.Length, 8)) + ")";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
