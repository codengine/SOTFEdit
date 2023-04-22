using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Map.Static;

namespace SOTFEdit.Model.Map;

public class ItemPoi : InformationalPoi
{
    private readonly IEnumerable<Item>? _altItems;
    private readonly HashSet<int> _inventoryItems;

    private readonly Item _item;

    private ItemPoi(float x, float y, Position? teleport, Item item, string? description, string? screenshot,
        IEnumerable<Item>? requirements, IEnumerable<Item>? altItems, HashSet<int> inventoryItems,
        bool isUnderground = false,
        string? wikiLink = null) : base(x, y, teleport,
        item.Name, description, requirements, screenshot, isUnderground, wikiLink)
    {
        _item = item;
        _altItems = altItems;
        _inventoryItems = inventoryItems;
    }

    public ItemInInventoryWrapper Item => new(_item, HasItemInInventory(_item));

    public IEnumerable<ItemInInventoryWrapper>? AltItems =>
        !_altItems?.Any() ?? false
            ? null
            : _altItems?.Select(item => new ItemInInventoryWrapper(item, HasItemInInventory(item)));

    public bool HasAnyAltItems => _altItems?.Any() ?? false;

    public BitmapImage? IconBig => _item.ThumbnailBig ?? null;
    public override BitmapImage Icon => _item.ThumbnailMedium ?? "/images/icons/treasure.png".LoadAppLocalImage();
    public BitmapImage IconSmall => _item.ThumbnailMedium ?? "/images/icons/treasure.png".LoadAppLocalImage(24, 24);

    public static ItemPoi? Of(RawPoi rawPoi, ItemList itemList, HashSet<int> inventoryItems,
        AreaMaskManager areaMaskManager, bool enabled)
    {
        if (rawPoi.ItemId is not { } itemId || itemList.GetItem(itemId) is not { } item)
        {
            return null;
        }

        return new ItemPoi(
            rawPoi.X,
            rawPoi.Y,
            rawPoi.Teleport?.ToPosition(areaMaskManager),
            item,
            rawPoi.Description,
            rawPoi.Screenshot,
            itemList.GetItems(rawPoi.Requirements),
            itemList.GetItems(rawPoi.AltItemIds),
            inventoryItems,
            rawPoi.IsUnderground,
            rawPoi.Wiki
        )
        {
            Enabled = enabled,
            MissingRequiredItems = rawPoi.GetMissingRequiredItems(inventoryItems)
        };
    }

    public override void ApplyFilter(MapFilter mapFilter)
    {
        Filtered = ShouldFilter(mapFilter);
    }

    protected override bool ShouldFilter(MapFilter mapFilter)
    {
        return (mapFilter.ShowOnlyUncollectedItems && HasAllItemsInInventory()) || base.ShouldFilter(mapFilter);
    }

    private bool HasAllItemsInInventory()
    {
        return HasItemInInventory(_item) && (_altItems?.All(HasItemInInventory) ?? true);
    }

    private bool HasItemInInventory(Item item)
    {
        return _inventoryItems.Contains(item.Id);
    }
}