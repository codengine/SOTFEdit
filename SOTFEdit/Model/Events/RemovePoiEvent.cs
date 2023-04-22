using System.Collections.Generic;
using SOTFEdit.Model.Map;

namespace SOTFEdit.Model.Events;

public class RemovePoiEvent
{
    public RemovePoiEvent(List<IPoi> pois)
    {
        Pois = pois;
    }

    public List<IPoi> Pois { get; }
}