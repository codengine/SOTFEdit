using SOTFEdit.Model.Map;

namespace SOTFEdit.Model.Events;

public class ShowSpawnActorsWindowEvent(BasePoi poi)
{
    public BasePoi Poi { get; } = poi;
}