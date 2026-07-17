using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageManager.Services;

public interface IFileService
{
    string InitializeTempDirectory();
    string SaveImageToTemp(BitmapSource image);
    void DeleteFile(string filePath);
    void CleanupDirectory(string directory);
    void CleanupOrphanDirectories();
}

public interface IThumbnailService
{
    Task<ImageSource> GetThumbnailAsync(string filePath);
}

public interface IClipboardService
{
    void SetImage(BitmapSource image);
    void SetText(string text);
    bool ContainsImage();
    bool ContainsText();
    string GetText();
    ImageSource? GetImage();
}

public interface IPasteService
{
    Task PasteImageAsync(string filePath, Window ownerWindow);
    Task PasteImagesAsync(IEnumerable<string> filePaths, Window ownerWindow);
}

public interface IConfigService
{
    bool DeleteWithoutConfirm { get; set; }
    bool AutoCleanOnExit { get; set; }
    bool ShowInTaskbar { get; set; }
    string Theme { get; set; }
    string? CustomTempPath { get; set; }
    double Transparency { get; set; } // 0=全透明, 1=不透明
    string Language { get; set; }
    bool DisableBlur { get; set; }
    string BackgroundMode { get; set; } // Glass=毛玻璃, Mica=Win11云母, None=纯色
    bool PrivacyMode { get; set; }
    bool AlwaysOnTop { get; set; }
    bool DeleteAfterPaste { get; set; }
    bool AutoDetectClipboard { get; set; }
    bool AutoReturnToTarget { get; set; }
    bool AutoStart { get; set; }
    bool SkipUpdateReminder { get; set; }
    string FontFamily { get; set; }
    int FontWeight { get; set; } // 100-900
    string GetTempRoot();
    void Load();
    void Save();
}

public enum ToastType { Success, Warning, Error, Info }

public interface IToastService
{
    void SetContainer(Panel container);
    void Show(string message, ToastType type = ToastType.Info);
}
