using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace SOTFEdit.Model.SaveData;

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record ActorItemBlock(int ItemId, int TotalCount, List<JToken> UniqueItems);