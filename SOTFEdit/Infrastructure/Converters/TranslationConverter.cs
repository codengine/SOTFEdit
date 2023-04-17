using System;
using System.Globalization;
using System.Windows.Data;

namespace SOTFEdit.Infrastructure.Converters;

[ValueConversion(typeof(string), typeof(string))]
public class TranslationConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return Binding.DoNothing;
        }

        var key = parameter + value.ToString();
        return TranslationManager.Get(key);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}