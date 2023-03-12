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
        if (value is GenericSetting.DataType type && parameter is string paramStr)
        {
            if (type == GenericSetting.DataType.Boolean && paramStr == "Bool")
            {
                return Visibility.Visible;
            }

            if (type == GenericSetting.DataType.Integer && paramStr == "Int")
            {
                return Visibility.Visible;
            }

            if (type == GenericSetting.DataType.Enum && paramStr == "Combo")
            {
                return Visibility.Visible;
            }

            if (type == GenericSetting.DataType.String && paramStr == "String")
            {
                return Visibility.Visible;
            }

            if (type == GenericSetting.DataType.ReadOnly && paramStr == "ReadOnly")
            {
                return Visibility.Visible;
            }
        }

        return Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}