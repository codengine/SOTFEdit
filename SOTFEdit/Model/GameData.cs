using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SOTFEdit.Model.Storage;

namespace SOTFEdit.Model;

public class GameData
{
    public GameData(IEnumerable<Item> items, [JsonProperty("storages")] List<StorageDefinition> storageDefinitions,
        [JsonProperty("followers")] FollowerData followerData, Configuration config)
    {
        Items = new ItemList(items.OrderBy(item => item.Name));
        StorageDefinitions = storageDefinitions;
        FollowerData = followerData;
        Config = config;
    }

    public ItemList Items { get; }
    public List<StorageDefinition> StorageDefinitions { get; }
    public FollowerData FollowerData { get; }
    public Configuration Config { get; }
}

public record Configuration(string LatestTagUrl, string LatestReleaseUrl);

public record FollowerData(Dictionary<int, List<Outfit>> Outfits, Dictionary<int, int[]> EquippableItems)
{
    public List<Outfit> GetOutfits(int typeId)
    {
        return Outfits.GetValueOrDefault(typeId) ?? new List<Outfit>();
    }

    public List<Item> GetEquippableItems(int typeId, ItemList items)
    {
        return (EquippableItems.GetValueOrDefault(typeId) ?? Array.Empty<int>())
            .Select(items.GetItem)
            .Where(item => item is { })
            .Select(item => item!)
            .ToList();
    }
}

public record Outfit(int Id, string Name);