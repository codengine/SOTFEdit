using Newtonsoft.Json.Linq;

namespace SOTFEdit.Model;

public record UnfinishedGenericScrewStructure(int Id, Position Pos, JToken Rot, int Added);