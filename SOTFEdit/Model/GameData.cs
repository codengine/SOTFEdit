using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Storage;

namespace SOTFEdit.Model;

public class GameData
{
    public GameData(IEnumerable<Item> items, [JsonProperty("storages")] List<StorageDefinition> storageDefinitions,
        [JsonProperty("advancedStorages")] List<AdvancedStorageDefinition> advancedStorageDefinitions,
        [JsonProperty("followers")] FollowerData followerData, Configuration config, List<string> namedIntKeys,
        List<ActorType> actorTypes, List<ScrewStructure> screwStructures, List<Area> areas,
        List<ElementProfile> elementProfiles)
    {
        Items = new ItemList(items.OrderBy(item => item.Name));
        StorageDefinitions = storageDefinitions;
        AdvancedStorageDefinitions = advancedStorageDefinitions;
        FollowerData = followerData;
        Config = config;
        NamedIntKeys = namedIntKeys.OrderBy(key => key).ToList();
        ActorTypes = actorTypes;
        ScrewStructures = screwStructures;
        ElementProfiles = elementProfiles;
        AreaManager = new AreaMaskManager(areas);
    }

    public AreaMaskManager AreaManager { get; }
    public List<ScrewStructure> ScrewStructures { get; }
    public List<ElementProfile> ElementProfiles { get; }

    public List<ActorType> ActorTypes { get; }

    public ItemList Items { get; }
    public List<StorageDefinition> StorageDefinitions { get; }
    public List<AdvancedStorageDefinition> AdvancedStorageDefinitions { get; }
    public FollowerData FollowerData { get; }
    public Configuration Config { get; }
    public List<string> NamedIntKeys { get; }
}

public class ElementProfile(ElementProfileCategory category, int id, int goldPlatedId)
{
    public ElementProfileCategory Category { get; } = category;
    public int Id { get; } = id;
    public int GoldPlatedId { get; } = goldPlatedId;
}

public enum ElementProfileCategory
{
    Log,
    LogPlank,
    Stone,
    Item,
    Stick
}

public class ScrewStructure(
    string category, int id, int buildCost, bool? canFinish, string icon, bool? canEdit,
    bool? showOnMap, bool? isWeaponHolder)
{
    public string Category { get; } = category;

    public bool IsWeaponHolder { get; } = isWeaponHolder ?? false;

    public string Name => TranslationManager.Get("structures.types." + Id);

    public string CategoryName =>
        string.IsNullOrEmpty(Category)
            ? ""
            : TranslationManager.Get("structures.categories." + Category);

    public int Id { get; } = id;
    public int BuildCost { get; } = buildCost;
    public string Icon { get; } = icon;
    public bool CanFinish { get; } = canFinish ?? true;
    public bool ShowOnMap { get; } = showOnMap ?? true;
    public bool CanEdit { get; } = canEdit ?? true;
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Configuration(string githubProject)
{
    public string LatestTagUrl => $"https://api.github.com/repos/{githubProject}/releases/latest";
    public string ChangelogUrl => $"https://raw.githubusercontent.com/{githubProject}/master/CHANGELOG.md";
}

// ReSharper disable once ClassNeverInstantiated.Global
public class FollowerData
{
    private readonly Dictionary<int, int[]> _equippableItems;
    private readonly Dictionary<int, List<Outfit>> _outfits = new();

    public FollowerData(Dictionary<int, List<int>> outfits, Dictionary<int, int[]> equippableItems)
    {
        foreach (var typeIdToOutfits in outfits)
        foreach (var outfitId in typeIdToOutfits.Value)
        {
            _outfits.GetOrCreate(typeIdToOutfits.Key).Add(new Outfit(typeIdToOutfits.Key, outfitId));
        }

        _equippableItems = equippableItems;
    }

    public List<Outfit> GetOutfits(int typeId)
    {
        return _outfits.GetValueOrDefault(typeId) ?? [];
    }

    public IEnumerable<Item> GetEquippableItems(int typeId, ItemList items)
    {
        return (_equippableItems.GetValueOrDefault(typeId) ?? [])
            .Select(items.GetItem)
            .Where(item => item is not null)
            .Select(item => item!)
            .ToList();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Outfit(int typeId, int id)
{
    public int Id { get; } = id;

    public string Name => TranslationManager.Get($"followers.outfits.{typeId}.{Id}");
}