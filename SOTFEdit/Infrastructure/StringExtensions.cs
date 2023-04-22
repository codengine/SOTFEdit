using System.IO;
using System.Windows.Media.Imaging;

namespace SOTFEdit.Infrastructure;

public static class StringExtensions
{
    public static BitmapImage LoadAppLocalImage(this string pathRelativeToRoot, int? width = null, int? height = null)
    {
        return AppLocalImageCache.Get(pathRelativeToRoot, width, height);
    }

    public static string ExtendFilenameWith(this string filename, string extension)
    {
        return $"{Path.GetFileNameWithoutExtension(filename)}{extension}{Path.GetExtension(filename)}";
    }
}