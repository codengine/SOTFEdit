using SOTFEdit.Infrastructure.Companion;

namespace SOTFEdit.Model.Events;

public class CustomPoiAddedEvent(CustomPoi poi)
{
    public CustomPoi Poi { get; } = poi;
}