using SOTFEdit.Model.Map;

namespace SOTFEdit.Model.Events;

public class AddZipPoiEvent
{
    public AddZipPoiEvent(ZiplinePoi poi)
    {
        Poi = poi;
    }

    public ZiplinePoi Poi { get; }
}