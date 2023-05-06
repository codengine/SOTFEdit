using System;
using System.Collections.Concurrent;
using System.Windows.Media.Imaging;

namespace SOTFEdit.Infrastructure;

public static class AppLocalImageCache
{
    private static readonly ConcurrentDictionary<CacheKey, BitmapImage> Cache = new();

    public static BitmapImage Get(string pathRelativeToRoot, int? width, int? height)
    {
        return Cache.GetOrAdd(new CacheKey(pathRelativeToRoot, width, height), LoadLocalImage);
    }

    private static BitmapImage LoadLocalImage(CacheKey cacheKey)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.None;
        if (cacheKey.Width is { } theWidth)
        {
            bitmap.DecodePixelWidth = theWidth;
        }

        if (cacheKey.Height is { } theHeight)
        {
            bitmap.DecodePixelWidth = theHeight;
        }

        bitmap.UriSource = new Uri($"pack://application:,,,/SOTFEdit;component{cacheKey.PathRelativeToRoot}");
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private class CacheKey
    {
        internal CacheKey(string pathRelativeToRoot, int? width, int? height)
        {
            PathRelativeToRoot = pathRelativeToRoot;
            Width = width;
            Height = height;
        }

        public string PathRelativeToRoot { get; }
        public int? Width { get; }
        public int? Height { get; }

        private bool Equals(CacheKey other)
        {
            return PathRelativeToRoot == other.PathRelativeToRoot && Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((CacheKey)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PathRelativeToRoot, Width, Height);
        }
    }
}