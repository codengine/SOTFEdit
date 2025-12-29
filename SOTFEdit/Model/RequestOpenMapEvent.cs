using System.Collections.Generic;
using SOTFEdit.Model.Map;

namespace SOTFEdit.Model;

public class RequestOpenMapEvent(List<IPoiGrouper> poiGroups)
{
    public List<IPoiGrouper> PoiGroups { get; } = poiGroups;
}