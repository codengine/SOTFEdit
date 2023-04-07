using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Storage.Module;

namespace SOTFEdit.View.Storage;

public class ItemWrapper
{
    private readonly int? _maxPerSlot;

    public ItemWrapper(Item item, int? maxPerSlot, ModuleWrapper? moduleWrapper = null)
    {
        _maxPerSlot = maxPerSlot;
        Item = item;
        ModuleWrapper = moduleWrapper;
    }

    public Item Item { get; }
    public ModuleWrapper? ModuleWrapper { get; }

    public string Name => ModuleWrapper?.Name ?? Item.Name;

    public int Max => _maxPerSlot ?? Item.StorageMax?.Shelf ?? 1;
}