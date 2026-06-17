using System.IO;
using System.Text.Json;

namespace ImageManager.Services;

public class ConfigService : IConfigService
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ImageManager");
    private static readonly string ConfigPath = Path.Combine(ConfigDir, "settings.json");

    public bool DeleteWithoutConfirm { get; set; }
    public bool AutoCleanOnExit { get; set; } = true;
    public bool ShowInTaskbar { get; set; }
    public string Theme { get; set; } = "System";
    public string? CustomTempPath { get; set; }
    public double Transparency { get; set; } = 0.75;

    public void Load()
    {
        if (!File.Exists(ConfigPath))
            return;

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var data = JsonSerializer.Deserialize<ConfigData>(json);
            if (data != null)
            {
                DeleteWithoutConfirm = data.DeleteWithoutConfirm;
                AutoCleanOnExit = data.AutoCleanOnExit;
                ShowInTaskbar = data.ShowInTaskbar;
                Theme = string.IsNullOrEmpty(data.Theme) ? "System" : data.Theme;
                CustomTempPath = string.IsNullOrEmpty(data.CustomTempPath) ? null : data.CustomTempPath;
                Transparency = data.Transparency > 0 && data.Transparency <= 1 ? data.Transparency : 0.75;
            }
        }
        catch { }
    }

    public void Save()
    {
        Directory.CreateDirectory(ConfigDir);
        var data = new ConfigData
        {
            DeleteWithoutConfirm = DeleteWithoutConfirm,
            AutoCleanOnExit = AutoCleanOnExit,
            ShowInTaskbar = ShowInTaskbar,
            Theme = Theme,
            CustomTempPath = CustomTempPath,
            Transparency = Transparency
        };
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }

    public string GetTempRoot()
    {
        return CustomTempPath ?? Path.Combine(Path.GetTempPath(), "ImageManager");
    }

    private class ConfigData
    {
        public bool DeleteWithoutConfirm { get; set; }
        public bool AutoCleanOnExit { get; set; }
        public bool ShowInTaskbar { get; set; }
        public string? Theme { get; set; }
        public string? CustomTempPath { get; set; }
        public double Transparency { get; set; } = 0.75;
    }
}
