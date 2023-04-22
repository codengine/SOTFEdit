using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SOTFEdit.Infrastructure.Converters;

public class BoolToBorderBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected ? Brushes.Black : Brushes.Transparent;
        }

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}