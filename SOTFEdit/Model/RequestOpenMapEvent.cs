using System.Collections.Generic;
using SOTFEdit.Model.Map;

namespace SOTFEdit.Model;

public class RequestOpenMapEvent
{
    public RequestOpenMapEvent(List<IPoiGrouper> poiGroups)
    {
        PoiGroups = poiGroups;
    }

    public List<IPoiGrouper> PoiGroups { get; }
}