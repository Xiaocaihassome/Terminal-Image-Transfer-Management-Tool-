using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using ImageManager.Services;
using ImageManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ImageManager;

public partial class App : Application
{
    private Mutex? _mutex;
    private bool _hasMutex;
    private ServiceProvider? _serviceProvider;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const string MutexName = "ImageManager_SingleInstance";
    private const int SW_RESTORE = 9;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 单实例检查
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        _hasMutex = createdNew;

        if (!createdNew)
        {
            // 激活已有窗口
            var current = Process.GetCurrentProcess();
            var existing = Process.GetProcessesByName(current.ProcessName)
                .FirstOrDefault(p => p.Id != current.Id);

            if (existing?.MainWindowHandle != IntPtr.Zero)
            {
                ShowWindow(existing.MainWindowHandle, SW_RESTORE);
                SetForegroundWindow(existing.MainWindowHandle);
            }

            Shutdown();
            return;
        }

        // 全局异常处理
        SetupExceptionHandling();

        // DI 容器
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // 启动清理孤儿目录
        var fileService = _serviceProvider.GetRequiredService<IFileService>();
        fileService.CleanupOrphanDirectories();

        // 应用保存的主题
        var configService = _serviceProvider.GetRequiredService<IConfigService>();
        var settingsVm = _serviceProvider.GetRequiredService<SettingsViewModel>();

        // 应用保存的语言
        LanguageManager.ApplyLanguage(configService.Language);

        settingsVm.ApplyTheme(configService.Theme, configService.Transparency);

        // 应用保存的字体
        if (!string.IsNullOrEmpty(configService.FontFamily))
        {
            Resources["GlobalFontFamily"] = new System.Windows.Media.FontFamily(configService.FontFamily);
            Resources["GlobalFontWeight"] = MapFontWeight(configService.FontWeight);
        }

        // 显示主窗口
        var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
        var mainWindow = new MainWindow(
              mainVm,
              _serviceProvider.GetRequiredService<IToastService>(),
              _serviceProvider.GetRequiredService<IPasteService>(),
              _serviceProvider.GetRequiredService<IConfigService>(),
              _serviceProvider,
              _serviceProvider.GetRequiredService<IFileService>());

        mainWindow.Show();

        // 后台静默检查更新（3秒后）
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);

            if (configService.SkipUpdateReminder) return;

            var updateService = _serviceProvider.GetRequiredService<IUpdateService>();
            var update = await updateService.CheckAsync();
            if (update != null)
            {
                settingsVm.LatestUpdate = update;
                settingsVm.HasUpdate = true;
                mainVm.HasUpdate = true;
            }
        });

        base.OnStartup(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IThumbnailService, ThumbnailService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IToastService, ToastService>();
        services.AddSingleton<IPasteService, PasteService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<IErrorLogService, ErrorLogService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SettingsViewModel>();
    }

    private void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            LogError(args.ExceptionObject as Exception);
            MessageBox.Show("程序遇到未知错误，请重试", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        };

        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            LogError(args.Exception);
            args.SetObserved();
        };

        DispatcherUnhandledException += (s, args) =>
        {
            LogError(args.Exception);
            args.Handled = true;
        };
    }

    private static void LogError(Exception? ex)
    {
        if (ex == null) return;

        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ImageManager", "Logs");
            Directory.CreateDirectory(logDir);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logFile = Path.Combine(logDir, $"error_{DateTime.Now:yyyyMMdd}.log");

            var text = $"[{timestamp}] {ex.GetType().Name}: {ex.Message}";
            if (ex.InnerException != null)
                text += $"\n  Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
            text += $"\n  StackTrace: {ex.StackTrace}\n\n";

            File.AppendAllText(logFile, text);

            // 复制 AI 修复提示词到剪贴板
            var prompt = "Please fix this error in my C# WPF application.\n\n" +
                         "## Project\n" +
                         "- Name: ImageManager (Terminal Image Transfer Management Tool)\n" +
                         "- Tech: C# WPF / .NET 8\n" +
                         "- Source: https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-\n" +
                         "- Docs: https://imagemanager-6gs.pages.dev/docs.html\n\n" +
                         "## Error\n" +
                         $"Type: {ex.GetType().Name}\n" +
                         $"Message: {ex.Message}\n" +
                         $"Inner: {ex.InnerException?.Message}\n\n" +
                         "## Stack Trace\n" +
                         $"{ex.StackTrace}\n\n" +
                         "Please provide the fix with code changes. Reference the source code and docs if needed.";
            try { Clipboard.SetText(prompt); } catch { }
        }
        catch { }
    }

    private static System.Windows.FontWeight MapFontWeight(int w) => w switch
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

    protected override void OnExit(ExitEventArgs e)
    {
        if (_hasMutex)
        {
            _mutex?.ReleaseMutex();
        }
        _mutex?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

