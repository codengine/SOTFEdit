using System.Collections.Generic;

namespace SOTFEdit.Model.Map.Static;

// ReSharper disable once ClassNeverInstantiated.Global
public record RawItemPoiCollection(
    Dictionary<string, string> OverridingTypeIcons,
    HashSet<string> DefaultEnabledGroups,
    HashSet<string> AllowedGroupsForCompanion,
    Dictionary<int, RawItemPoiGroup> Items
);