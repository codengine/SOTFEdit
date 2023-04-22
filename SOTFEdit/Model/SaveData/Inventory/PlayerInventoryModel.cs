using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model.SaveData.Storage.Module;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.SaveData.Inventory;

// ReSharper disable once ClassNeverInstantiated.Global
public record PlayerInventoryModel
{
    public ItemInstanceManagerDataModel ItemInstanceManagerData { get; set; }

    public static bool Merge(JToken playerInventory, IEnumerable<InventoryItem> selectedItems)
    {
        var selectedItemsDict = selectedItems.ToDictionary(inventoryItem => inventoryItem.Id);

        if (playerInventory.SelectToken("ItemInstanceManagerData.ItemBlocks") is not JArray itemBlocks)
        {
            return false;
        }

        List<JToken> finalTokens = new();
        HashSet<int> processedItemIds = new();

        var hasChanges = false;

        foreach (var itemBlock in itemBlocks)
        {
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
                        if (selectedItem.Item is { } item)
                        {
                            UpdateUniqueItems(itemBlock, totalCountToken?.ToObject<int>() ?? 0, selectedItem, item);
                        }

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
        }

        var newItemsFromDict = selectedItemsDict.Values.Where(item => !processedItemIds.Contains(item.Id))
            .Select(inventoryItem =>
            {
                var block = JToken.FromObject(inventoryItem.ItemBlock);

                if (inventoryItem.Item is { } item)
                {
                    UpdateUniqueItems(block, 0, inventoryItem, item);
                }

                return block;
            })
            .ToList();

        finalTokens.AddRange(newItemsFromDict);

        hasChanges = hasChanges || newItemsFromDict.Any();

        if (hasChanges)
        {
            itemBlocks.ReplaceAll(finalTokens.ToArray());
        }

        return hasChanges;
    }

    private static void UpdateUniqueItems(JToken itemBlock, int previousTotalCount, InventoryItem selectedItem,
        Item item)
    {
        if (previousTotalCount == selectedItem.TotalCount || !item.HasModules())
        {
            return;
        }

        if (itemBlock["UniqueItems"] is not JArray uniqueItems)
        {
            uniqueItems = new JArray();
        }

        var uniqueItemsCount = uniqueItems.Count;
        if (uniqueItemsCount > selectedItem.TotalCount)
        {
            var retainedItems = uniqueItems.Take(new Range(0, selectedItem.TotalCount)).ToList();
            itemBlock["UniqueItems"] = new JArray(retainedItems);
        }
        else
        {
            var modules = new List<IStorageModule>();
            if (item.FoodSpoilModuleDefinition is { } foodSpoilModule)
            {
                modules.Add(foodSpoilModule.BuildNewModuleWithDefaults());
            }

            if (item.SourceActorModuleDefinition is { } sourceActorModuleDefinition)
            {
                modules.Add(sourceActorModuleDefinition.BuildNewModuleWithDefaults());
            }

            for (var i = 0; i < selectedItem.TotalCount - uniqueItemsCount; i++)
            {
                uniqueItems.Add(new JObject
                {
                    { "Modules", new JArray(modules.Select(JToken.FromObject).ToList()) }
                });
            }

            itemBlock["UniqueItems"] = uniqueItems;
        }
    }
}