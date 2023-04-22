using System;
using System.Globalization;
using System.Windows.Data;

namespace SOTFEdit.Infrastructure.Converters;

public class ItemThumbWithFallbackConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value ?? "/images/icons/treasure.png".LoadAppLocalImage();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}