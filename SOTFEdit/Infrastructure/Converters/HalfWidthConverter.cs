using System;
using System.Globalization;
using System.Windows.Data;

namespace SOTFEdit.Infrastructure.Converters;

public class HalfWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue / 2;
        }

        throw new InvalidOperationException("Invalid type. Expected a double.");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}