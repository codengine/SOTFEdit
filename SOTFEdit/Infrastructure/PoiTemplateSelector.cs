using System.Windows;
using System.Windows.Controls;
using SOTFEdit.Model.Map;

namespace SOTFEdit.Infrastructure;

public class PoiTemplateSelector : DataTemplateSelector
{
    public DataTemplate? PoiTemplate { get; set; }
    public DataTemplate? NetworkPlayerPoiTemplate { get; set; }
    public DataTemplate? ZiplineTemplate { get; set; }
    public DataTemplate? ZipPoiTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item switch
        {
            ZiplinePoi => ZiplineTemplate,
            ZipPointPoi => ZipPoiTemplate,
            NetworkPlayerPoi => NetworkPlayerPoiTemplate,
            _ => PoiTemplate
        };
    }
}