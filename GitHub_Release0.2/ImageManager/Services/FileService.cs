using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageManager.Services;

public class FileService : IFileService
{
    private readonly IConfigService _configService;
    private string _currentDir = string.Empty;

    public FileService(IConfigService configService)
    {
        _configService = configService;
    }

    public string InitializeTempDirectory()
    {
        var root = _configService.GetTempRoot();
        Directory.CreateDirectory(root);

        foreach (var dir in Directory.GetDirectories(root))
        {
            var dirName = Path.GetFileName(dir);
            if (int.TryParse(dirName, out int pid))
            {
                try
                {
                    var process = Process.GetProcessById(pid);
                    if (process.HasExited)
                        Directory.Delete(dir, true);
                }
                catch
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        _currentDir = Path.Combine(root, Environment.ProcessId.ToString());
        Directory.CreateDirectory(_currentDir);
        return _currentDir;
    }

    public string SaveImageToTemp(BitmapSource image)
    {
        var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
        var filePath = Path.Combine(_currentDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        encoder.Save(stream);

        return filePath;
    }

    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public void CleanupDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        foreach (var file in Directory.GetFiles(directory))
        {
            try { File.Delete(file); } catch { }
        }

        try { Directory.Delete(directory); } catch { }
    }

    public void CleanupOrphanDirectories()
    {
        var root = _configService.GetTempRoot();
        if (!Directory.Exists(root))
            return;

        foreach (var dir in Directory.GetDirectories(root))
        {
            var dirName = Path.GetFileName(dir);
            if (int.TryParse(dirName, out int pid))
            {
                try
                {
                    var process = Process.GetProcessById(pid);
                    if (process.HasExited)
                        Directory.Delete(dir, true);
                }
                catch
                {
                    Directory.Delete(dir, true);
                }
            }
        }
    }
}
