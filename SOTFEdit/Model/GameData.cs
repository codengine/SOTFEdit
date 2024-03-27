using System;
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

public class ElementProfile
{
    public ElementProfile(ElementProfileCategory category, int id, int goldPlatedId)
    {
        Category = category;
        Id = id;
        GoldPlatedId = goldPlatedId;
    }

    public ElementProfileCategory Category { get; }
    public int Id { get; }
    public int GoldPlatedId { get; }
}

public enum ElementProfileCategory
{
    Log,
    LogPlank,
    Stone,
    Item,
    Stick
}

public class ScrewStructure
{
    public ScrewStructure(string category, int id, int buildCost, bool? canFinish, string icon, bool? canEdit,
        bool? showOnMap, bool? isWeaponHolder)
    {
        Category = category;
        Id = id;
        BuildCost = buildCost;
        Icon = icon;
        CanFinish = canFinish ?? true;
        CanEdit = canEdit ?? true;
        IsWeaponHolder = isWeaponHolder ?? false;
        ShowOnMap = showOnMap ?? true;
    }

    public string Category { get; }

    public bool IsWeaponHolder { get; }

    public string Name => TranslationManager.Get("structures.types." + Id);

    public string CategoryName => string.IsNullOrEmpty(Category)
        ? ""
        : TranslationManager.Get("structures.categories." + Category);

    public int Id { get; }
    public int BuildCost { get; }
    public string Icon { get; }
    public bool CanFinish { get; }
    public bool ShowOnMap { get; }
    public bool CanEdit { get; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Configuration
{
    private readonly string _githubProject;

    public Configuration(string githubProject)
    {
        _githubProject = githubProject;
    }

    public string LatestTagUrl => $"https://api.github.com/repos/{_githubProject}/releases/latest";
    public string ChangelogUrl => $"https://raw.githubusercontent.com/{_githubProject}/master/CHANGELOG.md";
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
        return _outfits.GetValueOrDefault(typeId) ?? new List<Outfit>();
    }

    public IEnumerable<Item> GetEquippableItems(int typeId, ItemList items)
    {
        return (_equippableItems.GetValueOrDefault(typeId) ?? Array.Empty<int>())
            .Select(items.GetItem)
            .Where(item => item is not null)
            .Select(item => item!)
            .ToList();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Outfit
{
    private readonly int _typeId;

    public Outfit(int typeId, int id)
    {
        _typeId = typeId;
        Id = id;
    }

    public int Id { get; }

    public string Name => TranslationManager.Get($"followers.outfits.{_typeId}.{Id}");
}