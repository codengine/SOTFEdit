using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Storage.Module;

namespace SOTFEdit.View.Storage;

public class ItemWrapper
{
    private readonly int? _maxPerSlot;

    public ItemWrapper(Item item, int? maxPerSlot, FoodSpoilStorageModuleWrapper? foodSpoilStorageModuleWrapper = null)
    {
        _maxPerSlot = maxPerSlot;
        Item = item;
        FoodSpoilStorageModuleWrapper = foodSpoilStorageModuleWrapper;
    }

    public Item Item { get; }
    public FoodSpoilStorageModuleWrapper? FoodSpoilStorageModuleWrapper { get; }

    public string Name => FoodSpoilStorageModuleWrapper?.Name ?? Item.Name;

    public int Max => _maxPerSlot ?? Item.StorageMax?.Shelf ?? 1;
}