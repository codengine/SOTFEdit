using SOTFEdit.Model.Map;

namespace SOTFEdit.Model.Events;

public class ReapplyPoiFilterEvent(IPoi poi)
{
    public IPoi Poi { get; } = poi;
}