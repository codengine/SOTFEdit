using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SOTFEdit.Model.SaveData;

public record ActorItemBlock(int ItemId, int TotalCount, List<JToken> UniqueItems);