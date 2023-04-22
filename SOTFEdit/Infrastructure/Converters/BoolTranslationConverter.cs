using System;
using System.Globalization;
using System.Windows.Data;

namespace SOTFEdit.Infrastructure.Converters;

public class BoolTranslationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not bool b ? "" : TranslationManager.Get("generic." + (b ? "yes" : "no"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}