using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Infrastructure;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Map.Static;

public class PoiLoader
{
    private readonly AreaMaskManager _areaMaskManager;
    private readonly CompanionPoiStorage _companionPoiStorage;
    private readonly InventoryPageViewModel _inventoryPageViewModel;
    private readonly ItemList _items;
    private readonly PlayerPageViewModel _playerPageViewModel;
    private readonly ConcurrentDictionary<PoiGroupType, IPoiGrouper> _rawPoiCache = new();

    public PoiLoader(GameData gameData, InventoryPageViewModel inventoryPageViewModel,
        PlayerPageViewModel playerPageViewModel,
        CompanionPoiStorage companionPoiStorage)
    {
        _items = gameData.Items;
        _areaMaskManager = gameData.AreaManager;
        _inventoryPageViewModel = inventoryPageViewModel;
        _playerPageViewModel = playerPageViewModel;
        _companionPoiStorage = companionPoiStorage;
    }

    public IPoiGrouper? GetRawPois(PoiGroupType type)
    {
        if (_rawPoiCache.IsEmpty)
        {
            var inventoryItems = GetInventoryItems();
            LoadRawPois(inventoryItems);
        }

        return _rawPoiCache.GetValueOrDefault(type);
    }

    public IEnumerable<IPoiGrouper> Load()
    {
        var inventoryItems = GetInventoryItems();
        var rawPois = LoadRawPois(inventoryItems);

        return new List<IPoiGrouper>(rawPois)
        {
            LoadItemPois(inventoryItems),
            LoadCustomPois()
        };
    }

    private HashSet<int> GetInventoryItems()
    {
        var itemsWithHashes = _items.GetItemsWithHashes();

        var inventoryItems = _inventoryPageViewModel.InventoryCollectionView.OfType<InventoryItem>()
            .SelectMany(item =>
            {
                var itemIds = new HashSet<int> { item.Id };
                AddItemIdsFromWeaponMods(item, itemIds, itemsWithHashes);

                return itemIds;
            })
            .ToHashSet();

        if (_playerPageViewModel.PlayerState.SelectedCloth is { } selectedCloth)
        {
            inventoryItems.Add(selectedCloth.Id);
        }

        return inventoryItems;
    }

    private static void AddItemIdsFromWeaponMods(InventoryItem item, ISet<int> itemIds,
        IReadOnlyCollection<Item> itemsWithHashes)
    {
        List<JToken> modules;

        if (item.ItemBlock.Modules is { Count: > 0 } m)
        {
            modules = m;
        }
        else if (item.ItemBlock.UniqueItems is { Count: > 0 } uniqueItems)
        {
            modules = new List<JToken>();
            foreach (var uniqueItem in uniqueItems)
            {
                if (uniqueItem["Modules"] is JArray mUnique)
                {
                    modules.AddRange(mUnique);
                }
            }
        }
        else
        {
            return;
        }

        foreach (var module in modules)
        {
            if (module["WeaponModIds"] is not JArray weaponModIds)
            {
                continue;
            }

            foreach (var weaponModId in weaponModIds)
            {
                var modItemId = weaponModId.Value<int>();

                foreach (
                    var itemId in
                    from itemWithHashes in itemsWithHashes
                    where itemWithHashes.ModHashes?.Contains(modItemId) ?? false
                    select itemWithHashes.Id)
                {
                    itemIds.Add(itemId);
                    break;
                }
            }
        }
    }

    private IPoiGrouper LoadCustomPois()
    {
        var customMapPois = _companionPoiStorage.GetAll()
            .Select(poi => CustomMapPoi.FromCustomPoi(poi, _areaMaskManager))
            .ToList();

        return new PoiGroup(true, customMapPois, TranslationManager.Get("map.customPois"), PoiGroupType.Custom,
            CustomMapPoi.CategoryIcon);
    }

    public IPoiGrouper GetItemPoisForCompanion()
    {
        var inventoryItems = GetInventoryItems();
        return LoadItemPois(inventoryItems, true);
    }

    private IPoiGrouper LoadItemPois(HashSet<int> inventoryItems, bool filterForCompanion = false)
    {
        var rawPoiCollection = JsonConverter.DeserializeFromFile<RawItemPoiCollection>(
                                   Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "item_pois.json")) ??
                               throw new Exception("Unable to find item pois");

        var poisByType = rawPoiCollection.Items.SelectMany(kvp => kvp.Value.Pois.Select(poi =>
                ItemPoi.Of(kvp.Key, kvp.Value, poi, _items, inventoryItems, _areaMaskManager, false)))
            .Where(poi => poi != null)
            .Select(poi => poi!)
            .Where(poi =>
                !filterForCompanion || rawPoiCollection.AllowedGroupsForCompanion.Contains(poi.Item.Item.Type))
            .GroupBy(poi => poi.Item.Item.Type)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());

        var groups = new List<PoiGroup>();

        foreach (var (type, pois) in poisByType)
        {
            var icon = rawPoiCollection.OverridingTypeIcons.GetValueOrDefault(type)?.LoadAppLocalImage(24, 24) ??
                       pois.First().IconSmall;

            var isEnabled = rawPoiCollection.DefaultEnabledGroups.Contains(type);
            if (isEnabled)
            {
                pois.ForEach(poi => poi.SetEnabledNoRefresh(isEnabled));
            }

            groups.Add(new PoiGroup(isEnabled, pois, TranslationManager.Get($"itemTypes.{type}"),
                PoiGroupKeys.Items + type,
                PoiGroupType.Items, icon));
        }

        return new PoiGroupCollection(false, TranslationManager.Get("poiGroups.Items"), PoiGroupKeys.Items, groups,
            PoiGroupType.Items);
    }

    private List<IPoiGrouper> LoadRawPois(HashSet<int> inventoryItems)
    {
        var result = new List<IPoiGrouper>();

        var rawPois = JsonConverter.DeserializeFromFile<Dictionary<string, RawPoiGroup>>(
                          Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "pois.json")) ??
                      new Dictionary<string, RawPoiGroup>();

        foreach (var (_, group) in rawPois)
        {
            var title = TranslationManager.Get($"poiGroups.{group.Type}");

            switch (group.Type)
            {
                case PoiGroupType.Bunkers:
                case PoiGroupType.Caves:
                    var caveOrBunkerPois =
                        group.Pois.Select(poi => CaveOrBunkerPoi.Of(poi, _items, group.Icon!, inventoryItems,
                            _areaMaskManager, group.AlwaysEnabled)).ToList();
                    result.Add(new PoiGroup(group.AlwaysEnabled, caveOrBunkerPois, title, group.Type,
                        caveOrBunkerPois.First().IconSmall));
                    break;
                case PoiGroupType.Printers:
                case PoiGroupType.Laptops:
                case PoiGroupType.Camps:
                case PoiGroupType.Villages:
                case PoiGroupType.Helicopters:
                case PoiGroupType.Info:
                case PoiGroupType.Doors:
                case PoiGroupType.Crates:
                case PoiGroupType.Supply:
                case PoiGroupType.Ammo:
                case PoiGroupType.CannibalVillages:
                case PoiGroupType.Ponds:
                case PoiGroupType.Lakes:
                    var informationalPois = group.Pois
                        .Select(poi => DefaultGenericInformationalPoi.Of(poi, _items, group.Icon!, inventoryItems,
                            _areaMaskManager, group.AlwaysEnabled))
                        .ToList();
                    result.Add(new PoiGroup(group.AlwaysEnabled,
                        informationalPois,
                        title, group.Type, informationalPois.First().IconSmall));
                    break;
                case PoiGroupType.Generic:
                case PoiGroupType.Custom:
                case PoiGroupType.Items:
                case PoiGroupType.Actors:
                case PoiGroupType.WorldItems:
                case PoiGroupType.Structures:
                case PoiGroupType.ZipLines:
                case PoiGroupType.Player:
                case PoiGroupType.Followers:
                default:
                    continue;
            }
        }

        result.ForEach(g => _rawPoiCache[g.GroupType] = g);

        return result;
    }
}