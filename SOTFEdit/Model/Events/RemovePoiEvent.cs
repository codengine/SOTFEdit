using System.Collections.Generic;
using SOTFEdit.Model.Map;

namespace SOTFEdit.Model.Events;

public class RemovePoiEvent(List<IPoi> pois)
{
    public List<IPoi> Pois { get; } = pois;
}