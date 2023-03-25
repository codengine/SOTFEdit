using System.Collections.Generic;

// ReSharper disable NotAccessedPositionalProperty.Global

namespace SOTFEdit.Model.SaveData.Actor;

public record InfluenceMemory(int UniqueId, List<Influence> Influences);