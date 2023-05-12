using System.Windows;
using System.Windows.Controls;
using SOTFEdit.Model.Map;

namespace SOTFEdit.Infrastructure;

public class PoiDetailsTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ItemPoiDetailsTemplate { get; set; }
    public DataTemplate? CaveOrBunkerPoiDetailsTemplate { get; set; }
    public DataTemplate? GenericInformationalPoiDetailsTemplate { get; set; }
    public DataTemplate? WorldItemPoiDetailsTemplate { get; set; }
    public DataTemplate? ActorPoiDetailsTemplate { get; set; }
    public DataTemplate? StructurePoiDetailsTemplate { get; set; }
    public DataTemplate? ZipPointPoiDetailsTemplate { get; set; }
    public DataTemplate? CustomPoiDetailsTemplate { get; set; }
    public DataTemplate? DefaultPoiTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item switch
        {
            ItemPoi => ItemPoiDetailsTemplate,
            CaveOrBunkerPoi => CaveOrBunkerPoiDetailsTemplate,
            WorldItemPoi => WorldItemPoiDetailsTemplate,
            ActorPoi => ActorPoiDetailsTemplate,
            StructurePoi => StructurePoiDetailsTemplate,
            CustomMapPoi => CustomPoiDetailsTemplate,
            DefaultGenericInformationalPoi => GenericInformationalPoiDetailsTemplate,
            ZipPointPoi => ZipPointPoiDetailsTemplate,
            IPoi => DefaultPoiTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}