using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Storage.Module;

namespace SOTFEdit.View.Storage;

public class ItemWrapper
{
    private readonly int? _maxPerSlot;
    private readonly bool _preferHolder;

    public ItemWrapper(Item item, int? maxPerSlot, bool? preferHolder = false,
        FoodSpoilStorageModuleWrapper? foodSpoilStorageModuleWrapper = null)
    {
        _maxPerSlot = maxPerSlot;
        _preferHolder = preferHolder ?? false;
        Item = item;
        FoodSpoilStorageModuleWrapper = foodSpoilStorageModuleWrapper;
    }

    public Item Item { get; }
    public FoodSpoilStorageModuleWrapper? FoodSpoilStorageModuleWrapper { get; }

    public string Name => FoodSpoilStorageModuleWrapper?.Name ?? Item.Name;

    public int Max => _maxPerSlot ?? (MaxPerSlotFromItemDefinition() ?? 1);

    private int? MaxPerSlotFromItemDefinition()
    {
        return _preferHolder ? MaxPerSlotPreferringHolder() : Item.StorageMax?.Shelf;
    }

    private int? MaxPerSlotPreferringHolder()
    {
        return Item.StorageMax?.Holder ?? Item.StorageMax?.Shelf;
    }
}