using System;
using System.Globalization;
using System.Windows.Data;
using SOTFEdit.Model;

namespace SOTFEdit.Infrastructure.Converters;

[ValueConversion(typeof(VegetationState), typeof(bool), ParameterType = typeof(VegetationState))]
public class VegetationStateToBooleanConverter : IValueConverter
{
    private VegetationState _target;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not VegetationState mask || value is not VegetationState target)
        {
            return Binding.DoNothing;
        }

        _target = target;
        return (mask & _target) != 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not VegetationState mask)
        {
            return Binding.DoNothing;
        }

        _target ^= mask;
        return _target;
    }
}