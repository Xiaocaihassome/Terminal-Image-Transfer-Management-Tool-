using System.IO;

namespace ImageManager.Services;

public interface IErrorLogService
{
    void Log(string message, Exception? ex = null);
    string GetLogDirectory();
}

public class ErrorLogService : IErrorLogService
{
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ImageManager", "Logs");

    private static readonly object _lock = new();

    public string GetLogDirectory() => LogDir;

    public void Log(string message, Exception? ex = null)
    {
        try
        {
            Directory.CreateDirectory(LogDir);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logFile = Path.Combine(LogDir, $"error_{DateTime.Now:yyyyMMdd}.log");

            var text = $"[{timestamp}] {message}";
            if (ex != null)
            {
                text += $"\n  Exception: {ex.GetType().Name}: {ex.Message}";
                if (ex.InnerException != null)
                    text += $"\n  Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
                text += $"\n  StackTrace: {ex.StackTrace}";
            }
            text += "\n\n";

            lock (_lock)
            {
                File.AppendAllText(logFile, text);
            }
        }
        catch { }
    }
}
