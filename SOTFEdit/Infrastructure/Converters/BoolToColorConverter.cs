using System;
using System.Globalization;
using System.Windows.Data;

namespace SOTFEdit.Infrastructure.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b && parameter is object[] p)
        {
            return b ? p[0] : p[1];
        }

        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}