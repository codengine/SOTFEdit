using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverter = SOTFEdit.Infrastructure.JsonConverter;

namespace SOTFEdit.Model.SaveData;

public record PlayerInventoryData
{
    public DataModel Data { get; set; }

    public static bool Merge(JToken target, List<ItemInstanceManagerData.ItemBlock> selectedItems)
    {
        var playerInventoryToken = target.SelectToken("Data.PlayerInventory");
        if (playerInventoryToken?.ToObject<string>() is not { } playerInventoryJson)
        {
            return false;
        }

        var playerInventory = JsonConverter.DeserializeRaw(playerInventoryJson);
        if (!PlayerInventory.Merge(playerInventory, selectedItems))
        {
            return false;
        }

        playerInventoryToken.Replace(JsonConverter.Serialize(playerInventory));
        return true;
    }

    public class DataModel
    {
        [JsonConverter(typeof(StringTypeConverter))]
        public PlayerInventory PlayerInventory { get; set; }
    }
}

public record PlayerInventory
{
    public ItemInstanceManagerData ItemInstanceManagerData { get; set; }

    public static bool Merge(JToken playerInventory, List<ItemInstanceManagerData.ItemBlock> selectedItems)
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
        {
            if (itemBlock["ItemId"]?.ToObject<int>() is { } itemId)
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

public record ItemInstanceManagerData : SotfBaseModel
{
    public List<ItemBlock> ItemBlocks { get; set; }

    public class ItemBlock
    {
        public List<JToken> UniqueItems = new();
        public int ItemId { get; set; }
        public int TotalCount { get; set; }

        public static ItemBlock Unassigned(int itemId)
        {
            return new ItemBlock
            {
                ItemId = itemId,
                TotalCount = 1
            };
        }
    }
}