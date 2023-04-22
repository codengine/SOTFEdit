using Newtonsoft.Json.Linq;

// ReSharper disable NotAccessedPositionalProperty.Global

namespace SOTFEdit.Model;

public record UnfinishedGenericScrewStructure(int Id, Position Pos, JToken Rot, int Added);