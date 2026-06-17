using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ImageManager.Services;

namespace ImageManager.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private string _currentTheme = "System";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTempPath))]
    private string? _customTempPath;

    [ObservableProperty]
    private double _transparency = 0.75;

    [ObservableProperty]
    private bool _deleteWithoutConfirm;

    [ObservableProperty]
    private bool _autoCleanOnExit = true;

    public string DefaultTempPath => Path.Combine(Path.GetTempPath(), "ImageManager");
    public string DisplayTempPath => string.IsNullOrEmpty(CustomTempPath)
        ? $"默认: {DefaultTempPath}"
        : $"自定义: {CustomTempPath}";
    public string Version => "1.0.0";

    public SettingsViewModel(IConfigService configService, IToastService toastService)
    {
        _configService = configService;
        _toastService = toastService;
        LoadFromConfig();
    }

    private void LoadFromConfig()
    {
        CurrentTheme = _configService.Theme;
        CustomTempPath = _configService.CustomTempPath;
        DeleteWithoutConfirm = _configService.DeleteWithoutConfirm;
        AutoCleanOnExit = _configService.AutoCleanOnExit;
        Transparency = _configService.Transparency;
    }

    partial void OnTransparencyChanged(double value)
    {
        _configService.Transparency = value;
        _configService.Save();
        ApplyTheme(CurrentTheme, value);
    }

    [RelayCommand]
    private void SetTheme(string theme)
    {
        CurrentTheme = theme;
        _configService.Theme = theme;
        _configService.Save();
        ApplyTheme(theme);
    }

    [RelayCommand]
    private void BrowseTempPath()
    {
        // 使用 OpenFileDialog 的文件夹选择模式（兼容性更好）
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "选择自定义临时文件夹位置",
            FileName = "Folder Selection",
            Filter = "文件夹|*.*",
            CheckFileExists = false,
            ValidateNames = false
        };

        if (dialog.ShowDialog() == true)
        {
            var selectedPath = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                CustomTempPath = selectedPath;
                _configService.CustomTempPath = selectedPath;
                _configService.Save();
                _toastService.Show("已设置自定义文件夹", ToastType.Success);
            }
        }
    }

    [RelayCommand]
    private void RestoreDefaultPath()
    {
        CustomTempPath = null;
        _configService.CustomTempPath = null;
        _configService.Save();
        _toastService.Show("已恢复默认文件夹", ToastType.Success);
    }

    [RelayCommand]
    private void OpenCurrentFolder()
    {
        var path = CustomTempPath ?? DefaultTempPath;
        if (Directory.Exists(path))
            Process.Start("explorer.exe", path);
        else
            _toastService.Show("文件夹不存在", ToastType.Warning);
    }

    [RelayCommand]
    private void OpenHelpUrl()
    {
        Process.Start(new ProcessStartInfo("https://github.com/xiaocaiyou-dianliao/ImageManager") { UseShellExecute = true });
    }

    public void ApplyTheme(string theme, double? transparency = null)
    {
        ThemeManager.Instance.ApplyTheme(theme, transparency ?? Transparency);
    }
}
