using System;
using System.Globalization;
using System.Windows.Data;

namespace SOTFEdit.Infrastructure.Converters;

[ValueConversion(typeof(string), typeof(bool))]
public class StringToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string stringValue)
        {
            return false;
        }

        return stringValue == (string)parameter;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? parameter : Binding.DoNothing;
    }
}