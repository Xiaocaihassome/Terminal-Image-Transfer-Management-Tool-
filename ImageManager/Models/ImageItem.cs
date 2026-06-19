using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ImageManager.Models;

public class ImageItem : INotifyPropertyChanged
{
    public string FilePath { get; }
    public string FileName => Path.GetFileName(FilePath);
    public DateTime CreatedTime { get; }

    private ImageSource? _thumbnail;
    public ImageSource? Thumbnail
    {
        get => _thumbnail;
        set { _thumbnail = value; OnPropertyChanged(); }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
                SelectionChanged?.Invoke(this, value);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<bool>? SelectionChanged;

    public ImageItem(string filePath)
    {
        FilePath = filePath;
        CreatedTime = File.GetLastWriteTime(filePath);
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
