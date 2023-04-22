using System.Collections.Generic;

namespace SOTFEdit.Model.Map;

public interface IPoiWithItems
{
    public IEnumerable<ItemInInventoryWrapper>? Items { get; }
    public bool HasAnyItems { get; }
}