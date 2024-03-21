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
    public List<ItemBlockModel>? EquippedItems { get; set; }
    public ItemInstanceManagerDataModel ItemInstanceManagerData { get; set; }

    public static bool Merge(JToken playerInventory, List<InventoryItem> selectedItems)
    {
        var hasChanges = false;
        HashSet<int> processedItemIds = new();

        if (playerInventory.SelectToken("EquippedItems") is JArray equippedItems)
        {
            hasChanges = ProcessEquippedItems(selectedItems, equippedItems, processedItemIds) || hasChanges;
        }

        //This always has to go last as it adds everything to inventory that has not been processed yet
        if (playerInventory.SelectToken("ItemInstanceManagerData.ItemBlocks") is JArray inventoryItems)
        {
            hasChanges = ProcessInventoryItems(selectedItems, inventoryItems, processedItemIds) || hasChanges;
        }

        if (playerInventory.SelectToken("QuickSelect.Slots") is JArray quickSelectSlots)
        {
            ProcessQuickSelectSlots(selectedItems, quickSelectSlots);
        }

        return hasChanges;
    }

    private static void ProcessQuickSelectSlots(IEnumerable<InventoryItem> selectedItems, JContainer quickSelectSlots)
    {
        var allIds = selectedItems.Select(item => item.Id).ToHashSet();
        var filteredTokens = quickSelectSlots
            .Where(token => token["ItemId"]?.Value<int>() is not { } itemId || allIds.Contains(itemId))
            .ToList();
        quickSelectSlots.ReplaceAll(filteredTokens);
    }

    private static bool ProcessInventoryItems(IReadOnlyCollection<InventoryItem> selectedItems, JArray inventoryItems,
        ISet<int> processedItemIds)
    {
        var selectedItemsDict = selectedItems.Where(item => !item.IsEquipped)
            .ToDictionary(item => item.Id);
        var hasChanges = ProcessItemBlocks(inventoryItems, selectedItemsDict, out var finalTokens, processedItemIds);

        var newItemsFromDict = selectedItems.Where(item => !processedItemIds.Contains(item.Id))
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
            inventoryItems.ReplaceAll(finalTokens);
        }

        return hasChanges;
    }

    private static bool ProcessEquippedItems(IEnumerable<InventoryItem> selectedItems, JArray equippedItems,
        ISet<int> processedItemIds)
    {
        var selectedItemsDict = selectedItems.Where(item => item.IsEquipped)
            .ToDictionary(item => item.Id);
        var hasChanges = ProcessItemBlocks(equippedItems, selectedItemsDict, out var finalTokens, processedItemIds);

        if (hasChanges)
        {
            equippedItems.ReplaceAll(finalTokens);
        }

        return hasChanges;
    }

    private static bool ProcessItemBlocks(JArray itemBlocks, IReadOnlyDictionary<int, InventoryItem> selectedItemsDict,
        out List<JToken> finalTokens, ISet<int> processedItemIds)
    {
        var hasChanges = false;
        finalTokens = new List<JToken>();
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

                    if (selectedItem.TotalCount > 0 || selectedItem.IsEquipped)
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

        return hasChanges;
    }

    private static void UpdateUniqueItems(JToken itemBlock, int previousTotalCount, InventoryItem selectedItem,
        Item item)
    {
        if (previousTotalCount == selectedItem.TotalCount || !item.HasFoodSpoilModuleDefinition())
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