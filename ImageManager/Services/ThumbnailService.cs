using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageManager.Services;

public class ThumbnailService : IThumbnailService
{
    public async Task<ImageSource> GetThumbnailAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            using var stream = new FileStream(filePath, FileMode.Open,
                FileAccess.Read, FileShare.Read);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.DecodePixelWidth = 96;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return (ImageSource)bitmap;
        });
    }
}
