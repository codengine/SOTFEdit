using SOTFEdit.Model.Map;

namespace SOTFEdit.Model.Events;

public class ReapplyPoiFilterEvent
{
    public ReapplyPoiFilterEvent(IPoi poi)
    {
        Poi = poi;
    }

    public IPoi Poi { get; }
}