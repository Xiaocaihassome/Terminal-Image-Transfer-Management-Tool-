using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ImageManager.Services;

namespace ImageManager.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    private readonly IToastService _toastService;
    private readonly IUpdateService _updateService;
    private readonly MainViewModel? _mainViewModel;

    // 由 SettingsWindow 设置，用于切换设置后刷新毛玻璃效果
    public Action? RefreshBackdrop { get; set; }

    [ObservableProperty]
    private string _currentTheme = "System";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTempPath))]
    private string? _customTempPath;

    [ObservableProperty]
    private double _transparency = 0.75;

    [ObservableProperty]
    private string _currentLanguage = "zh-CN";

    [ObservableProperty]
    private bool _disableBlur;

    [ObservableProperty]
    private string _backgroundMode = "Glass";

    [ObservableProperty]
    private bool _deleteWithoutConfirm;

    [ObservableProperty]
    private bool _autoCleanOnExit = true;

    [ObservableProperty]
    private bool _privacyMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FontWeightLabel))]
    private string _selectedFontFamily = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FontWeightLabel))]
    private int _selectedFontWeight = 400;

    // 更新相关
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateButtonText))]
    private bool _hasUpdate;

    [ObservableProperty]
    private UpdateInfo? _latestUpdate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateButtonText))]
    private string _updateStatus = "";

    public string UpdateButtonText => HasUpdate ? $"★ {LatestUpdate?.Version}" : UpdateStatus;
    public string FontWeightLabel => SelectedFontWeight switch
    {
        100 => "Thin",
        200 => "ExtraLight",
        300 => "Light",
        400 => "Regular",
        500 => "Medium",
        600 => "SemiBold",
        700 => "Bold",
        800 => "ExtraBold",
        900 => "Black",
        _ => SelectedFontWeight.ToString()
    };
    public List<string> InstalledFonts { get; } = new();
    public List<int> FontWeights { get; } = new() { 100, 200, 300, 400, 500, 600, 700, 800, 900 };

    public string DefaultTempPath => Path.Combine(Path.GetTempPath(), "ImageManager");
    public string DisplayTempPath
    {
        get
        {
            if (PrivacyMode)
                return "默认: ********";
            return string.IsNullOrEmpty(CustomTempPath)
                ? $"默认: {DefaultTempPath}"
                : $"自定义: {CustomTempPath}";
        }
    }
    public string Version => "1.2.0";

    public SettingsViewModel(IConfigService configService, IToastService toastService, IUpdateService updateService, MainViewModel mainViewModel)
    {
        _configService = configService;
        _toastService = toastService;
        _updateService = updateService;
        _mainViewModel = mainViewModel;

        // 加载系统已安装字体
        foreach (var family in Fonts.SystemFontFamilies)
            InstalledFonts.Add(family.Source);

        LoadFromConfig();
    }

    private void LoadFromConfig()
    {
        CurrentTheme = _configService.Theme;
        CustomTempPath = _configService.CustomTempPath;
        DeleteWithoutConfirm = _configService.DeleteWithoutConfirm;
        AutoCleanOnExit = _configService.AutoCleanOnExit;
        Transparency = _configService.Transparency;
        CurrentLanguage = _configService.Language;
        DisableBlur = _configService.DisableBlur;
        BackgroundMode = _configService.BackgroundMode;
        PrivacyMode = _configService.PrivacyMode;
        AlwaysOnTop = _configService.AlwaysOnTop;
        SelectedFontFamily = _configService.FontFamily;
        SelectedFontWeight = _configService.FontWeight;
    }

    partial void OnTransparencyChanged(double value)
    {
        _configService.Transparency = value;
        _configService.Save();
        ApplyTheme(CurrentTheme, value);
        RefreshBackdrop?.Invoke();
    }

    partial void OnCurrentLanguageChanged(string value)
    {
        _configService.Language = value;
        _configService.Save();
        LanguageManager.ApplyLanguage(value);
    }

    partial void OnDisableBlurChanged(bool value)
    {
        _configService.DisableBlur = value;
        _configService.Save();
        RefreshBackdrop?.Invoke();
    }

    partial void OnPrivacyModeChanged(bool value)
    {
        _configService.PrivacyMode = value;
        _configService.Save();
        OnPropertyChanged(nameof(DisplayTempPath));
    }

    [ObservableProperty]
    private bool _alwaysOnTop;

    partial void OnAlwaysOnTopChanged(bool value)
    {
        _configService.AlwaysOnTop = value;
        _configService.Save();
        ApplyAlwaysOnTop?.Invoke(value);
    }

    public Action<bool>? ApplyAlwaysOnTop { get; set; }

    partial void OnSelectedFontFamilyChanged(string value)
    {
        _configService.FontFamily = value;
        _configService.Save();
        ApplyGlobalFont();
    }

    partial void OnSelectedFontWeightChanged(int value)
    {
        _configService.FontWeight = value;
        _configService.Save();
        ApplyGlobalFont();
    }

    public void ApplyGlobalFont()
    {
        var app = Application.Current;
        if (app == null) return;

        var family = string.IsNullOrEmpty(SelectedFontFamily)
            ? new FontFamily("Microsoft YaHei UI")
            : new FontFamily(SelectedFontFamily);

        app.Resources["GlobalFontFamily"] = family;
        app.Resources["GlobalFontWeight"] = SelectedFontWeight switch
        {
            100 => System.Windows.FontWeights.Thin,
            200 => System.Windows.FontWeights.ExtraLight,
            300 => System.Windows.FontWeights.Light,
            400 => System.Windows.FontWeights.Normal,
            500 => System.Windows.FontWeights.Medium,
            600 => System.Windows.FontWeights.SemiBold,
            700 => System.Windows.FontWeights.Bold,
            800 => System.Windows.FontWeights.ExtraBold,
            900 => System.Windows.FontWeights.Black,
            _ => System.Windows.FontWeights.Normal
        };
    }

    [RelayCommand]
    private void SetBackgroundMode(string mode)
    {
        BackgroundMode = mode;
        _configService.BackgroundMode = mode;
        // 同步旧字段，保持兼容
        DisableBlur = mode == "None";
        _configService.DisableBlur = DisableBlur;
        _configService.Save();
        RefreshBackdrop?.Invoke();
    }

    [RelayCommand]
    private void SetTheme(string theme)
    {
        CurrentTheme = theme;
        _configService.Theme = theme;
        _configService.Save();
        ApplyTheme(theme);
        RefreshBackdrop?.Invoke();
    }

    [RelayCommand]
    private void SetLanguage(string lang)
    {
        CurrentLanguage = lang;
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

    [RelayCommand]
    private async Task CheckUpdateAsync()
    {
        UpdateStatus = "检查中...";
        HasUpdate = false;
        LatestUpdate = null;

        var update = await _updateService.CheckAsync();
        if (update != null)
        {
            LatestUpdate = update;
            HasUpdate = true;
            if (_mainViewModel != null) _mainViewModel.HasUpdate = true;
            UpdateStatus = $"发现新版本 {update.Version}";
        }
        else
        {
            UpdateStatus = "已是最新版本";
        }
    }

    [RelayCommand]
    private async Task DownloadUpdateAsync()
    {
        if (LatestUpdate == null) return;

        UpdateStatus = "正在下载...";
        try
        {
            await _updateService.DownloadAndInstallAsync(LatestUpdate);
        }
        catch (Exception ex)
        {
            UpdateStatus = $"下载失败：{ex.Message}";
            _toastService.Show("下载失败，请手动下载", ToastType.Error);
        }
    }

    public void ApplyTheme(string theme, double? transparency = null)
    {
        ThemeManager.Instance.ApplyTheme(theme, transparency ?? Transparency);
    }
}
