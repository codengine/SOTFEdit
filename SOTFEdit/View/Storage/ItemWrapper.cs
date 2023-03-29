using SOTFEdit.Model;

namespace SOTFEdit.View.Storage;

public class ItemWrapper
{
    private readonly int? _maxPerSlot;

    public ItemWrapper(Item item, int? maxPerSlot, ItemVariant? variant = null, int? moduleId = null)
    {
        _maxPerSlot = maxPerSlot;
        Variant = variant;
        ModuleId = moduleId;
        Item = item;
    }

    public ItemVariant? Variant { get; }
    public int? ModuleId { get; }
    public Item Item { get; }

    public string Name => Variant?.Name ?? Item.Name;

    public int Max => _maxPerSlot ?? Item.StorageMax?.Shelf ?? 1;
}