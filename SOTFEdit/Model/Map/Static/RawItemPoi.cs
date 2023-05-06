using System.Collections.Generic;
using System.Linq;

namespace SOTFEdit.Model.Map.Static;

// ReSharper disable once ClassNeverInstantiated.Global
public record RawItemPoi(string? Description, float X, float Y, int[]? Requirements,
    int[]? AltItemIds, string? Screenshot, Teleport Teleport)
{
    public int[]? GetMissingRequiredItems(HashSet<int> inventoryItems)
    {
        return Requirements == null || Requirements.Length == 0
            ? null
            : Requirements.Where(itemId => !inventoryItems.Contains(itemId)).ToArray();
    }
}