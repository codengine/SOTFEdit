using System.Windows;
using System.Windows.Controls;
using SOTFEdit.Model;

namespace SOTFEdit.Infrastructure;

public class SettingCellTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ReadOnlyTemplate { get; set; }
    public DataTemplate? StringValueTemplate { get; set; }
    public DataTemplate? BooleanValueTemplate { get; set; }
    public DataTemplate? ComboValueTemplate { get; set; }
    public DataTemplate? NumericValueTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is not GenericSetting setting)
        {
            return ReadOnlyTemplate;
        }

        switch (setting.Type)
        {
            case GenericSetting.DataType.String:
                return StringValueTemplate;
            case GenericSetting.DataType.Boolean:
                return BooleanValueTemplate;
            case GenericSetting.DataType.Integer:
                return NumericValueTemplate;
            case GenericSetting.DataType.Enum:
                return ComboValueTemplate;
            case GenericSetting.DataType.ReadOnly:
            default:
                return ReadOnlyTemplate;
        }
    }
}