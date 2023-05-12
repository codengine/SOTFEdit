using System.Collections.Generic;
using System.Windows.Media.Imaging;
using SOTFEdit.Model.Map.Static;

namespace SOTFEdit.Model.Map;

public class DefaultGenericInformationalPoi : InformationalPoi
{
    private readonly string _icon;

    protected DefaultGenericInformationalPoi(float x, float y, Position? teleport, string title, string? description,
        string? screenshot,
        string icon,
        IEnumerable<Item>? requirements, bool isUnderground = false, string? wikiLink = null) : base(x, y, teleport,
        title,
        description,
        requirements, screenshot,
        isUnderground, wikiLink)
    {
        _icon = icon;
    }

    public override BitmapImage Icon => LoadBaseIcon(_icon);
    public BitmapImage IconSmall => LoadBaseIcon(_icon, 24, 24);

    public static DefaultGenericInformationalPoi Of(RawPoi rawPoi, ItemList itemList, string icon,
        HashSet<int> inventoryItems, AreaMaskManager areaMaskManager, bool enabled)
    {
        var poi = new DefaultGenericInformationalPoi(
            rawPoi.X,
            rawPoi.Y,
            rawPoi.Teleport?.ToPosition(areaMaskManager),
            rawPoi.Title ?? "",
            rawPoi.Description,
            rawPoi.Screenshot,
            icon,
            itemList.GetItems(rawPoi.Requirements),
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