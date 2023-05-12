using System.Collections.Generic;
using SOTFEdit.Companion.Shared;

namespace SOTFEdit.Model.Map.Static;

// ReSharper disable once ClassNeverInstantiated.Global
public record RawPoiGroup(string? Icon, List<RawPoi> Pois, bool AlwaysEnabled = false,
    PoiGroupType Type = PoiGroupType.Generic);