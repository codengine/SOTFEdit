using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Map.Static;

public class PoiLoader
{
    private readonly AreaMaskManager _areaMaskManager;
    private readonly InventoryPageViewModel _inventoryPageViewModel;
    private readonly ItemList _items;

    public PoiLoader(GameData gameData, InventoryPageViewModel inventoryPageViewModel)
    {
        _items = gameData.Items;
        _areaMaskManager = gameData.AreaManager;
        _inventoryPageViewModel = inventoryPageViewModel;
    }

    public IEnumerable<IPoiGrouper> Load()
    {
        var result = new List<IPoiGrouper>();

        var inventoryItems = new HashSet<int>();

        foreach (var inventoryItem in _inventoryPageViewModel.InventoryCollectionView.OfType<InventoryItem>())
        {
            inventoryItems.Add(inventoryItem.Id);
        }

        LoadRawPois(result, inventoryItems);
        LoadItemPois(result, inventoryItems);

        return result;
    }

    private void LoadItemPois(ICollection<IPoiGrouper> result, HashSet<int> inventoryItems)
    {
        var rawPoiCollection = JsonConverter.DeserializeFromFile<RawItemPoiCollection>(
                                   Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "item_pois.json")) ??
                               throw new Exception("Unable to find item pois");

        var poisByType = rawPoiCollection.Items.SelectMany(kvp => kvp.Value.Pois.Select(poi =>
                ItemPoi.Of(kvp.Key, kvp.Value, poi, _items, inventoryItems, _areaMaskManager, false)))
            .Where(poi => poi != null)
            .Select(poi => poi!)
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
                pois.ForEach(poi => poi.Enabled = isEnabled);
            }

            groups.Add(new PoiGroup(isEnabled, pois, TranslationManager.Get($"itemTypes.{type}"),
                icon,
                PoiGroupType.Items));
        }

        result.Add(new PoiGroupCollection(false, TranslationManager.Get("poiGroups.items"), groups,
            PoiGroupType.Items));
    }

    private void LoadRawPois(ICollection<IPoiGrouper> result, HashSet<int> inventoryItems)
    {
        var rawPois = JsonConverter.DeserializeFromFile<Dictionary<string, RawPoiGroup>>(
                          Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "pois.json")) ??
                      new Dictionary<string, RawPoiGroup>();

        foreach (var (category, group) in rawPois)
        {
            var title = TranslationManager.Get($"poiGroups.{category}");

            switch (category)
            {
                case "bunkers":
                case "caves":
                    var caveOrBunkerPois =
                        group.Pois.Select(poi => CaveOrBunkerPoi.Of(poi, _items, group.Icon!, inventoryItems,
                            _areaMaskManager, group.AlwaysEnabled)).ToList();
                    result.Add(new PoiGroup(group.AlwaysEnabled, caveOrBunkerPois, title,
                        caveOrBunkerPois.First().IconSmall));
                    break;
                default:
                    var informationalPois = group.Pois
                        .Select(poi => DefaultGenericInformationalPoi.Of(poi, _items, group.Icon!, inventoryItems,
                            _areaMaskManager, group.AlwaysEnabled))
                        .ToList();
                    result.Add(new PoiGroup(group.AlwaysEnabled,
                        informationalPois,
                        title, informationalPois.First().IconSmall));
                    break;
            }
        }
    }
}