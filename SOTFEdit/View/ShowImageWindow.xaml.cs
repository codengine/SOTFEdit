using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SOTFEdit.View;

public partial class ShowImageWindow
{
    public ShowImageWindow(Window owner, string imageSource, string title)
    {
        Owner = owner;
        InitializeComponent();
        Title = title;
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var desiredWidth = screenWidth * (2.0 / 3.0);
        var desiredHeight = screenHeight * (2.0 / 3.0);
        Title = title;
        Width = desiredWidth;
        Height = desiredHeight;
        LoadImageAsync(imageSource);
    }

    private void LoadImageAsync(string imageSource)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;

        var uri = new Uri(imageSource, UriKind.RelativeOrAbsolute);

        if (!uri.IsAbsoluteUri || uri.IsFile)
        {
            // Local file.
            bitmap.UriSource = uri;
            bitmap.EndInit();

            // No need to freeze the bitmap, because we're not using it across threads.
            // Just update the UI directly.
            Image.Source = bitmap;
            Image.Width = bitmap.PixelWidth;
            Image.Height = bitmap.PixelHeight;
        }
        else
        {
            // Remote URL.
            bitmap.UriSource = uri;
            bitmap.DownloadCompleted += Bitmap_DownloadCompleted;
            bitmap.EndInit();
        }
    }

    private void Bitmap_DownloadCompleted(object? sender, EventArgs e)
    {
        var bitmap = (BitmapImage?)sender;
        if (bitmap == null)
        {
            return;
        }

        bitmap.Freeze(); // This is necessary to make the BitmapImage usable on the UI thread.

        // Use the Dispatcher to update the UI thread.
        Application.Current.Dispatcher.Invoke(() =>
        {
            Image.Source = bitmap;
            Image.Width = bitmap.PixelWidth;
            Image.Height = bitmap.PixelHeight;
        });
    }
}