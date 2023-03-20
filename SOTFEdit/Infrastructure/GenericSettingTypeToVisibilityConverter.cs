using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SOTFEdit.Model;

namespace SOTFEdit.Infrastructure;

public class GenericSettingTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not GenericSetting.DataType type || parameter is not string paramStr)
        {
            return Visibility.Hidden;
        }

        switch (type)
        {
            case GenericSetting.DataType.Boolean when paramStr == "Bool":
            case GenericSetting.DataType.Integer when paramStr == "Int":
            case GenericSetting.DataType.Enum when paramStr == "Combo":
            case GenericSetting.DataType.String when paramStr == "String":
            case GenericSetting.DataType.ReadOnly when paramStr == "ReadOnly":
                return Visibility.Visible;
            default:
                return Visibility.Hidden;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}