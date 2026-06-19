using System.Windows;

namespace ImageManager.Services;

public static class LanguageManager
{
    private static readonly Dictionary<string, string> LanguageMap = new()
    {
        ["zh-CN"] = "pack://application:,,,/Resources/Lang/zh-CN.xaml",
        ["zh-TW"] = "pack://application:,,,/Resources/Lang/zh-TW.xaml",
        ["en-US"] = "pack://application:,,,/Resources/Lang/en-US.xaml",
        ["ko-KR"] = "pack://application:,,,/Resources/Lang/ko-KR.xaml",
        ["ja-JP"] = "pack://application:,,,/Resources/Lang/ja-JP.xaml",
    };

    public static void ApplyLanguage(string langCode)
    {
        if (!LanguageMap.TryGetValue(langCode, out var packUri))
            langCode = "zh-CN";
        packUri ??= LanguageMap["zh-CN"];

        var dict = new ResourceDictionary { Source = new Uri(packUri) };

        var app = Application.Current;
        if (app == null) return;

        // 查找并替换语言资源字典
        for (int i = app.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
        {
            if (app.Resources.MergedDictionaries[i].Source?.ToString().Contains("/Lang/") == true)
            {
                app.Resources.MergedDictionaries.RemoveAt(i);
            }
        }

        app.Resources.MergedDictionaries.Add(dict);
    }
}
