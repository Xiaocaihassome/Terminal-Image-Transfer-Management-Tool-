using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;

namespace ImageManager.Services;

public record UpdateInfo(string Version, string DownloadUrl, string Notes);

public interface IUpdateService
{
    /// <summary>启动时静默检查，不弹窗。</summary>
    Task<UpdateInfo?> CheckAsync();
    /// <summary>下载安装包并启动安装。</summary>
    Task DownloadAndInstallAsync(UpdateInfo info);
}

public class UpdateService : IUpdateService
{
    private const string RepoApi = "https://api.github.com/repos/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-/releases/latest";
    private const string CurrentVersion = "1.1.1";

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(10),
        DefaultRequestHeaders =
        {
            { "User-Agent", "ImageManager" }
        }
    };

    public async Task<UpdateInfo?> CheckAsync()
    {
        try
        {
            var json = await Http.GetStringAsync(RepoApi);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.GetProperty("tag_name").GetString() ?? "";
            var version = tagName.TrimStart('v', 'V');
            var notes = root.TryGetProperty("body", out var body) ? body.GetString() ?? "" : "";

            if (new Version(version) <= new Version(CurrentVersion))
                return null;

            // 找第一个 .exe 资产
            string? downloadUrl = null;
            if (root.TryGetProperty("assets", out var assets))
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString() ?? "";
                    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString();
                        break;
                    }
                }
            }

            return downloadUrl == null ? null : new UpdateInfo(version, downloadUrl, notes);
        }
        catch
        {
            return null;
        }
    }

    public async Task DownloadAndInstallAsync(UpdateInfo info)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ImageManager", "update");
        Directory.CreateDirectory(tempDir);

        var fileName = $"ImageManager_Setup_{info.Version}.exe";
        var filePath = Path.Combine(tempDir, fileName);

        if (!File.Exists(filePath))
        {
            using var response = await Http.GetAsync(info.DownloadUrl);
            response.EnsureSuccessStatusCode();
            await using var fs = File.Create(filePath);
            await response.Content.CopyToAsync(fs);
        }

        // 启动安装包（静默模式），当前进程会被安装程序关闭
        Process.Start(new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        });

        // 退出当前应用，让安装程序接管
        Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
    }
}
