using System;
using System.Globalization;
using System.Windows.Data;

namespace SOTFEdit.Infrastructure.Converters;

[ValueConversion(typeof(string), typeof(bool))]
public class StringToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? parameter : Binding.DoNothing;
    }
}