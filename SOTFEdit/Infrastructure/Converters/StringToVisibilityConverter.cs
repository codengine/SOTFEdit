using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SOTFEdit.Infrastructure.Converters;

[ValueConversion(typeof(string), typeof(Visibility))]
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var strValue = value as string;
        return strValue == (string)parameter ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}