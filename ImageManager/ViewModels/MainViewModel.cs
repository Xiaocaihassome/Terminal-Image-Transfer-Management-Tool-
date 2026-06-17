using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageManager.Models;
using ImageManager.Services;

namespace ImageManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly IThumbnailService _thumbnailService;
    private readonly IClipboardService _clipboardService;
    private readonly IPasteService _pasteService;
    private readonly IConfigService _configService;
    private readonly IToastService _toastService;
    private string _tempDir = string.Empty;
    private bool _suppressUpdate; // 防止 UpdateSelectedCount ↔ OnIsAllSelectedChanged 互相触发

    public ObservableCollection<ImageItem> ImageItems { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAny))]
    private bool _isAllSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAny))]
    private bool _deleteWithoutConfirm;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAny))]
    private bool _autoCleanOnExit = true;

    [ObservableProperty]
    private string _selectedFilePath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAny))]
    private int _totalCount;

    [ObservableProperty]
    private int _selectedCount;

    [ObservableProperty]
    private bool _isBusy;

    public bool IsAny => TotalCount > 0;

    public MainViewModel(
        IFileService fileService,
        IThumbnailService thumbnailService,
        IClipboardService clipboardService,
        IPasteService pasteService,
        IConfigService configService,
        IToastService toastService)
    {
        _fileService = fileService;
        _thumbnailService = thumbnailService;
        _clipboardService = clipboardService;
        _pasteService = pasteService;
        _configService = configService;
        _toastService = toastService;

        configService.Load();
        DeleteWithoutConfirm = configService.DeleteWithoutConfirm;
        AutoCleanOnExit = configService.AutoCleanOnExit;

        _tempDir = fileService.InitializeTempDirectory();
    }

    partial void OnDeleteWithoutConfirmChanged(bool value)
    {
        _configService.DeleteWithoutConfirm = value;
        _configService.Save();
    }

    partial void OnAutoCleanOnExitChanged(bool value)
    {
        _configService.AutoCleanOnExit = value;
        _configService.Save();
    }

    partial void OnIsAllSelectedChanged(bool value)
    {
        if (_suppressUpdate) return;
        _suppressUpdate = true;
        foreach (var item in ImageItems)
            item.IsSelected = value;
        UpdateSelectedCount();
        _suppressUpdate = false;
    }

    public async Task AddImageAsync(string filePath)
    {
        if (!File.Exists(filePath)) return;
        if (ImageItems.Any(i => i.FilePath == filePath)) return;

        var item = new ImageItem(filePath) { IsSelected = true };
        item.SelectionChanged += (_, _) => UpdateSelectedCount();

        ImageItems.Insert(0, item);
        TotalCount = ImageItems.Count;

        // 异步加载缩略图
        try
        {
            item.Thumbnail = await _thumbnailService.GetThumbnailAsync(filePath);
        }
        catch { }
    }

    public async Task AddImagesAsync(IEnumerable<string> filePaths)
    {
        foreach (var path in filePaths)
        {
            var ext = Path.GetExtension(path).ToLower();
            if (new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".tiff" }.Contains(ext))
                await AddImageAsync(path);
        }
    }

    public void HandlePaste()
    {
        if (_clipboardService.ContainsImage())
        {
            var image = _clipboardService.GetImage();
            if (image != null)
            {
                var path = _fileService.SaveImageToTemp((System.Windows.Media.Imaging.BitmapSource)image);
                _ = AddImageAsync(path);
                _toastService.Show("已保存截图", ToastType.Success);
            }
        }
        else
        {
            _toastService.Show("剪贴板中没有图片", ToastType.Warning);
        }
    }

    [RelayCommand]
    private void CopySelectedPaths()
    {
        var selected = ImageItems.Where(i => i.IsSelected).ToList();
        if (selected.Count == 0)
        {
            _toastService.Show("请先选中要复制的文件", ToastType.Warning);
            return;
        }

        var text = string.Join("\n", selected.Select(i => i.FilePath));
        _clipboardService.SetText(text);
        _toastService.Show($"已复制 {selected.Count} 个文件路径", ToastType.Success);
    }

    [RelayCommand]
    private async Task PasteToWindowAsync(Window window)
    {
        var selected = ImageItems.FirstOrDefault(i => i.IsSelected);
        if (selected == null)
        {
            _toastService.Show("请先选中要粘贴的图片", ToastType.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            await _pasteService.PasteImageAsync(selected.FilePath, window);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        var selected = ImageItems.Where(i => i.IsSelected).ToList();
        if (selected.Count == 0) return;

        if (!DeleteWithoutConfirm)
        {
            var result = MessageBox.Show(
                $"确定删除选中的 {selected.Count} 个文件？",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
        }

        foreach (var item in selected)
        {
            _fileService.DeleteFile(item.FilePath);
            ImageItems.Remove(item);
        }

        TotalCount = ImageItems.Count;
        UpdateSelectedCount();
        _toastService.Show($"已删除 {selected.Count} 个文件", ToastType.Success);
    }

    [RelayCommand]
    private void ClearAll()
    {
        if (ImageItems.Count == 0) return;

        if (!DeleteWithoutConfirm)
        {
            var result = MessageBox.Show(
                $"确定清空全部 {ImageItems.Count} 个文件？",
                "确认清空",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
        }

        foreach (var item in ImageItems)
            _fileService.DeleteFile(item.FilePath);

        ImageItems.Clear();
        TotalCount = 0;
        UpdateSelectedCount();
        _toastService.Show("已清空全部文件", ToastType.Success);
    }

    [RelayCommand]
    private void CopyImageToClipboard(ImageItem? item)
    {
        if (item == null || !File.Exists(item.FilePath)) return;
        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(item.FilePath);
        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        _clipboardService.SetImage(bitmap);
        _toastService.Show("已复制图片", ToastType.Success);
    }

    [RelayCommand]
    private void OpenContainingFolder(ImageItem? item)
    {
        if (item == null) return;
        var dir = Path.GetDirectoryName(item.FilePath);
        if (dir != null && Directory.Exists(dir))
            Process.Start("explorer.exe", dir);
    }

    [RelayCommand]
    private void RenameItem(ImageItem? item)
    {
        if (item == null) return;
        var dir = Path.GetDirectoryName(item.FilePath);
        var ext = Path.GetExtension(item.FilePath);

        var input = new InputWindow("重命名", "新文件名：", Path.GetFileNameWithoutExtension(item.FilePath));
        if (input.ShowDialog() == true && !string.IsNullOrWhiteSpace(input.InputText))
        {
            var newPath = Path.Combine(dir!, input.InputText + ext);
            try
            {
                File.Move(item.FilePath, newPath);
                var idx = ImageItems.IndexOf(item);
                ImageItems[idx] = new ImageItem(newPath)
                {
                    Thumbnail = item.Thumbnail,
                    IsSelected = item.IsSelected
                };
                _toastService.Show("重命名成功", ToastType.Success);
            }
            catch (Exception ex)
            {
                _toastService.Show($"重命名失败：{ex.Message}", ToastType.Error);
            }
        }
    }

    public void SyncFromConfig(IConfigService config)
    {
        DeleteWithoutConfirm = config.DeleteWithoutConfirm;
        AutoCleanOnExit = config.AutoCleanOnExit;
    }

    public void CleanupOnExit()
    {
        if (AutoCleanOnExit)
            _fileService.CleanupDirectory(_tempDir);
    }

    private void UpdateSelectedCount()
    {
        SelectedCount = ImageItems.Count(i => i.IsSelected);

        if (_suppressUpdate) return;
        _suppressUpdate = true;
        IsAllSelected = ImageItems.Count > 0 && ImageItems.All(i => i.IsSelected);
        _suppressUpdate = false;
    }
}

// 简单输入框窗口
public class InputWindow : Window
{
    public string InputText { get; private set; } = string.Empty;

    public InputWindow(string title, string label, string defaultValue = "")
    {
        Title = title;
        Width = 360;
        Height = 160;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var margin = new Thickness(16);
        var stack = new StackPanel { Margin = margin };

        var labelBlock = new TextBlock { Text = label };
        labelBlock.Margin = new Thickness(0, 0, 0, 8);
        stack.Children.Add(labelBlock);

        var textBox = new TextBox();
        textBox.Text = defaultValue;
        textBox.Margin = new Thickness(0, 0, 0, 12);
        stack.Children.Add(textBox);

        var btnPanel = new StackPanel();
        btnPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
        btnPanel.HorizontalAlignment = HorizontalAlignment.Right;

        var okBtn = new Button();
        okBtn.Content = "确定";
        okBtn.Width = 80;
        okBtn.Margin = new Thickness(0, 0, 8, 0);
        okBtn.IsDefault = true;
        okBtn.Click += (_, _) => { InputText = textBox.Text; DialogResult = true; };

        var cancelBtn = new Button();
        cancelBtn.Content = "取消";
        cancelBtn.Width = 80;
        cancelBtn.IsCancel = true;

        btnPanel.Children.Add(okBtn);
        btnPanel.Children.Add(cancelBtn);
        stack.Children.Add(btnPanel);

        Content = stack;
        textBox.Focus();
        textBox.SelectAll();
    }
}
