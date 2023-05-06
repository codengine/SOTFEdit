using System.Collections.Generic;

namespace SOTFEdit.Model.Map.Static;

// ReSharper disable once ClassNeverInstantiated.Global
public record RawItemPoiGroup(List<RawItemPoi> Pois, string? Wiki = null);