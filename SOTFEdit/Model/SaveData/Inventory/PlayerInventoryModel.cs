using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SOTFEdit.Model.SaveData.Inventory;

// ReSharper disable once ClassNeverInstantiated.Global
public record PlayerInventoryModel
{
    public ItemInstanceManagerDataModel ItemInstanceManagerData { get; set; }

    public static bool Merge(JToken playerInventory, IEnumerable<ItemBlockModel> selectedItems)
    {
        var selectedItemsDict = selectedItems.ToDictionary(itemBlock => itemBlock.ItemId);

        if (playerInventory.SelectToken("ItemInstanceManagerData.ItemBlocks") is not JArray itemBlocks)
        {
            return false;
        }

        List<JToken> finalTokens = new();
        HashSet<int> processedItemIds = new();

        var hasChanges = false;

        foreach (var itemBlock in itemBlocks)
            if (itemBlock["ItemId"]?.Value<int>() is { } itemId)
            {
                if (selectedItemsDict.TryGetValue(itemId, out var selectedItem))
                {
                    var totalCountToken = itemBlock["TotalCount"];
                    var selectedItemTotalCount = JToken.FromObject(selectedItem.TotalCount);
                    if (totalCountToken != null && !totalCountToken.Equals(selectedItemTotalCount))
                    {
                        totalCountToken.Replace(selectedItemTotalCount);
                        hasChanges = true;
                    }

                    if (selectedItem.TotalCount > 0)
                    {
                        finalTokens.Add(itemBlock);
                    }
                }
                else
                {
                    hasChanges = true;
                }

                processedItemIds.Add(itemId);
            }
            else
            {
                finalTokens.Add(itemBlock);
            }

        var newItemsFromDict = selectedItemsDict.Values.Where(item => !processedItemIds.Contains(item.ItemId))
            .Select(JToken.FromObject)
            .ToList();

        finalTokens.AddRange(newItemsFromDict);

        hasChanges = hasChanges || newItemsFromDict.Any();

        if (hasChanges)
        {
            itemBlocks.ReplaceAll(finalTokens.ToArray());
        }

        return hasChanges;
    }
}