using System;
using System.Globalization;
using System.Windows.Data;
using SOTFEdit.Model;

namespace SOTFEdit.Infrastructure.Converters;

[ValueConversion(typeof(VegetationState), typeof(bool), ParameterType = typeof(VegetationState))]
public class VegetationStateToBooleanConverter : IValueConverter
{
    private VegetationState _target;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var mask = (VegetationState)parameter;
        _target = (VegetationState)value;
        return (mask & _target) != 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        _target ^= (VegetationState)parameter;
        return _target;
    }
}