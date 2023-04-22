using System.Windows.Media;

namespace SOTFEdit.Model.Map;

public class ItemInInventoryWrapper
{
    private readonly bool _hasItem;

    public ItemInInventoryWrapper(Item item, bool hasItem)
    {
        _hasItem = hasItem;
        Item = item;
    }

    public Item Item { get; }

    public Brush Color => _hasItem ? Brushes.ForestGreen : Brushes.DarkRed;
}