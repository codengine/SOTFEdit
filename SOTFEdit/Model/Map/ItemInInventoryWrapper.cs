using System.Windows.Media;

namespace SOTFEdit.Model.Map;

public class ItemInInventoryWrapper(Item item, bool hasItem)
{
    public Item Item { get; } = item;

    public Brush Color => hasItem ? Brushes.ForestGreen : Brushes.DarkRed;
}