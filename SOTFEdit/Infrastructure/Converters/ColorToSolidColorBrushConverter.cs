using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SOTFEdit.Infrastructure.Converters;

[ValueConversion(typeof(Color), typeof(SolidColorBrush))]
public class ColorToSolidColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Color color)
        {
            return Binding.DoNothing;
        }

        var brush = new SolidColorBrush(color);

        if (parameter is string stringParam && double.TryParse(stringParam, out var opacity))
        {
            brush.Opacity = opacity;
        }

        brush.Freeze();
        return brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is SolidColorBrush brush ? brush.Color : Binding.DoNothing;
    }
}