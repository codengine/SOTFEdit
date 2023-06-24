using Newtonsoft.Json;

namespace SOTFEdit.Model.WorldItem;

public record WorldItemState(string ObjectNameId, [property: JsonIgnore] string Group, Position Position,
    WorldItemType WorldItemType);