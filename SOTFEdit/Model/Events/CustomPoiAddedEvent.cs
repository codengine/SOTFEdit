using SOTFEdit.Infrastructure.Companion;

namespace SOTFEdit.Model.Events;

public class CustomPoiAddedEvent
{
    public CustomPoiAddedEvent(CustomPoi poi)
    {
        Poi = poi;
    }

    public CustomPoi Poi { get; }
}