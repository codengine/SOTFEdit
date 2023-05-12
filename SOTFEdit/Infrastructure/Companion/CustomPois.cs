using System.Collections.Generic;
using Newtonsoft.Json;

namespace SOTFEdit.Infrastructure.Companion;

public record CustomPois([property: JsonProperty("pois")] List<CustomPoi> Pois);