using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SOTFEdit.Infrastructure;

/// <summary>
///     Fast image resizing using Windows Presentation Foundation (WPF)
///     rendering support. Needs assembly references to PresentationCore
///     and WindowsBase.
/// </summary>
public class ImageResizing : IDisposable
{
    #region Fields

    private readonly JpegBitmapEncoder _jpegEncoder;
    private readonly Stream _sourceImageStream;
    private readonly BitmapFrame _firstImageBitmapFrame;

    #endregion

    #region Constructors

    public ImageResizing(string sourceImagePath)
        : this(new MemoryStream(File.ReadAllBytes(sourceImagePath)))
    {
    }

    public ImageResizing(byte[] sourceImageBytes)
        : this(new MemoryStream(sourceImageBytes))
    {
    }

    public ImageResizing(Stream sourceImageStream)
    {
        _sourceImageStream = sourceImageStream;
        _firstImageBitmapFrame = GetFirstBitmapFrame(_sourceImageStream);

        _jpegEncoder = new JpegBitmapEncoder();
        _jpegEncoder.Frames.Add(_firstImageBitmapFrame);
    }

    #endregion

    #region IImageResizer Members

    /// <summary>
    ///     Compresses the image source to the supplied quality threshold.
    /// </summary>
    /// <param name="quality"></param>
    /// <returns></returns>
    public ImageResizing Quality(int quality)
    {
        // Seems that 75 or less is the magic threshold 
        // for image compression using the JpegEncoder. 
        // Above this value, file size tends to increase 
        // over the original image file size.
        if (quality <= 75)
        {
            _jpegEncoder.QualityLevel = quality;
        }

        // Make this method chainable
        return this;
    }

    /// <summary>
    ///     Resizes the image source using the supplied width and height.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public ImageResizing Resize(int width, int height)
    {
        var resizedBitmapFrame = Resize(_firstImageBitmapFrame, width, height);

        _jpegEncoder.Frames.Clear();
        _jpegEncoder.Frames.Add(resizedBitmapFrame);

        // Make this method chainable
        return this;
    }

    /// <summary>
    ///     Saves the modified image source to the supplied destination path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="dispose"></param>
    public void Save(string path, bool dispose = true)
    {
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write)) _jpegEncoder.Save(fs);

        if (dispose)
        {
            Dispose();
        }
    }

    /// <summary>
    ///     Encodes the modified image to an image stream,
    ///     using the current jpeg encoder.
    /// </summary>
    /// <returns></returns>
    public MemoryStream ToStream()
    {
        var memStream = new MemoryStream();
        _jpegEncoder.Save(memStream);
        return memStream;
    }

    #endregion

    #region IDisposable Support

    private bool _disposedValue; // To detect redundant calls

    private void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing)
        {
            _sourceImageStream.Dispose();
        }

        _disposedValue = true;
    }

    public void Dispose()
    {
        // Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    ///     The image resizing method.
    /// </summary>
    /// <param name="bitmapFrame"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    private static BitmapFrame Resize(BitmapSource bitmapFrame, int width, int height)
    {
        double scaleWidth, scaleHeight;

        // Resize proportionally to the width
        if (height == 0)
        {
            scaleWidth = width;
            scaleHeight = (double)width / bitmapFrame.PixelWidth * bitmapFrame.PixelHeight;
        }
        // Resize proportionally to the height
        else if (width == 0)
        {
            scaleHeight = height;
            scaleWidth = (double)height / bitmapFrame.PixelHeight * bitmapFrame.PixelWidth;
        }
        // Resize using the supplied width and height
        else
        {
            scaleWidth = width;
            scaleHeight = height;
        }

        // Create the scale transform
        var scaleTransform = new ScaleTransform(scaleWidth / bitmapFrame.PixelWidth,
            scaleHeight / bitmapFrame.PixelHeight, 0, 0);

        // Transform the bitmap frame
        var transformedBitmap = new TransformedBitmap(bitmapFrame, scaleTransform);

        return BitmapFrame.Create(transformedBitmap);
    }

    /// <summary>
    ///     Reads out the first <see cref="BitmapFrame" /> from the supplied image stream.
    /// </summary>
    /// <param name="imageStream"></param>
    /// <returns></returns>
    private static BitmapFrame GetFirstBitmapFrame(Stream imageStream)
    {
        var bitmapDecoder = BitmapDecoder.Create(imageStream,
            BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
        return bitmapDecoder.Frames[0];
    }

    #endregion
}