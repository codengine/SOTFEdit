using System.Collections.Generic;
using System.Linq;
using SOTFEdit.Model.Map.Static;

namespace SOTFEdit.Model.Map;

public class CaveOrBunkerPoi : DefaultGenericInformationalPoi, IPoiWithItems
{
    private readonly HashSet<int> _inventoryItems;
    private readonly IEnumerable<Item>? _items;
    private readonly IEnumerable<string>? _objects;

    private CaveOrBunkerPoi(float x, float y, Position? teleport, string title, string? description,
        string? screenshot, string icon,
        IEnumerable<Item>? requirements, IEnumerable<Item>? items, HashSet<int> inventoryItems,
        IEnumerable<string>? objects,
        bool isUnderground = false,
        string? wikiLink = null) :
        base(x, y, teleport, title, description, screenshot, icon, requirements, isUnderground, wikiLink)
    {
        _items = items;
        _objects = objects;
        _inventoryItems = inventoryItems;
    }

    public string? Objects => _objects == null || !_objects.Any() ? null : string.Join(", ", _objects);

    public IEnumerable<ItemInInventoryWrapper>? Items => !_items?.Any() ?? false
        ? null
        : _items?.Select(item => new ItemInInventoryWrapper(item, HasItemInInventory(item)));

    public bool HasAnyItems => Items?.Any() ?? false;

    private bool HasItemInInventory(Item item)
    {
        return _inventoryItems.Contains(item.Id);
    }

    public override void ApplyFilter(MapFilter mapFilter)
    {
        Filtered = ShouldFilter(mapFilter);
    }

    protected override bool ShouldFilter(MapFilter mapFilter)
    {
        return (mapFilter.ShowOnlyUncollectedItems && HasAllItemsInInventory()) || base.ShouldFilter(mapFilter);
    }

    protected override bool FullTextFilter(string normalizedLowercaseFullText)
    {
        return base.FullTextFilter(normalizedLowercaseFullText) && !AnyItemContains(normalizedLowercaseFullText);
    }

    private bool AnyItemContains(string normalizedLowercaseFullText)
    {
        return Items?.Any(item => item.Item.Matches(normalizedLowercaseFullText)) ?? false;
    }

    private bool HasAllItemsInInventory()
    {
        return _items?.All(HasItemInInventory) ?? true;
    }

    public new static CaveOrBunkerPoi Of(RawPoi rawPoi, ItemList itemList, string icon, HashSet<int> inventoryItems,
        AreaMaskManager areaMaskManager, bool enabled)
    {
        var poi = new CaveOrBunkerPoi(
            rawPoi.X,
            rawPoi.Y,
            rawPoi.Teleport?.ToPosition(areaMaskManager),
            rawPoi.Title ?? "",
            rawPoi.Description,
            rawPoi.Screenshot,
            icon,
            itemList.GetItems(rawPoi.Requirements),
            itemList.GetItems(rawPoi.Items),
            inventoryItems,
            rawPoi.Objects,
            rawPoi.IsUnderground,
            rawPoi.Wiki
        )
        {
            MissingRequiredItems = rawPoi.GetMissingRequiredItems(inventoryItems)
        };

        poi.SetEnabledNoRefresh(enabled);
        return poi;
    }
}