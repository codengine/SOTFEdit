using SOTFEdit.Model.Map;

namespace SOTFEdit.Model.Events;

public class PoiRefreshEvent(IPoiGrouper grouper)
{
    public IPoiGrouper Grouper { get; } = grouper;
}