using SOTFEdit.Model.Map;

namespace SOTFEdit.Model.Events;

public class ShowSpawnActorsWindowEvent
{
    public ShowSpawnActorsWindowEvent(BasePoi poi)
    {
        Poi = poi;
    }

    public BasePoi Poi { get; }
}