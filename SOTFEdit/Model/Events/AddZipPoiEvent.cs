using SOTFEdit.Model.Map;

namespace SOTFEdit.Model.Events;

public class AddZipPoiEvent(ZiplinePoi poi)
{
    public ZiplinePoi Poi { get; } = poi;
}