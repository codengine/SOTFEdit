using System.Collections.Generic;

namespace SOTFEdit.Model.Map.Static;

// ReSharper disable once ClassNeverInstantiated.Global
public record RawPoiGroup(string? Icon, List<RawPoi> Pois, bool AlwaysEnabled = false);