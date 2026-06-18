using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageManager.Services;

public class ClipboardService : IClipboardService
{
    public void SetImage(BitmapSource image)
    {
        Application.Current.Dispatcher.Invoke(() => Clipboard.SetImage(image));
    }

    public void SetText(string text)
    {
        Application.Current.Dispatcher.Invoke(() => Clipboard.SetText(text));
    }

    public bool ContainsImage()
    {
        return Application.Current.Dispatcher.Invoke(() => Clipboard.ContainsImage());
    }

    public bool ContainsText()
    {
        return Application.Current.Dispatcher.Invoke(() => Clipboard.ContainsText());
    }

    public string GetText()
    {
        return Application.Current.Dispatcher.Invoke(() => Clipboard.GetText());
    }

    public ImageSource? GetImage()
    {
        return Application.Current.Dispatcher.Invoke(() =>
            Clipboard.ContainsImage() ? Clipboard.GetImage() : null);
    }
}
