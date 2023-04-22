using SOTFEdit.Model.Map;

namespace SOTFEdit.Model.Events;

public class PoiRefreshEvent
{
    public PoiRefreshEvent(IPoiGrouper grouper)
    {
        Grouper = grouper;
    }

    public IPoiGrouper Grouper { get; }
}