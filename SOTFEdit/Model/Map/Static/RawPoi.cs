using System.Collections.Generic;
using System.Linq;

namespace SOTFEdit.Model.Map.Static;

// ReSharper disable once ClassNeverInstantiated.Global
public record RawPoi(int? ItemId, string? Title, string? Description, float X, float Y, int[]? Requirements,
    int[]? AltItemIds, int[]? Items, string[]? Objects, string? Screenshot, bool IsUnderground = false,
    string? Wiki = null, Teleport? Teleport = null)
{
    public int[]? GetMissingRequiredItems(HashSet<int> inventoryItems)
    {
        return Requirements == null || Requirements.Length == 0
            ? null
            : Requirements.Where(itemId => !inventoryItems.Contains(itemId)).ToArray();
    }
}