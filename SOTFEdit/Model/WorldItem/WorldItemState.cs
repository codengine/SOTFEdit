using Newtonsoft.Json;

namespace SOTFEdit.Model.WorldItem;

public record WorldItemState(string ObjectName, [property: JsonIgnore] string Group, Position Position,
    WorldItemType WorldItemType, bool IsRuntimeCreated);