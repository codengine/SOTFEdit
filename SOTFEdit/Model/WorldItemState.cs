using Newtonsoft.Json;

namespace SOTFEdit.Model;

public record WorldItemState(int ItemId, string ObjectNameId, [property: JsonIgnore] string Group, Position Position);