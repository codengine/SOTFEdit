using System.Collections.Generic;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Storage;

// ReSharper disable once ClassNeverInstantiated.Global
public class AdvancedStorageDefinition(int id, List<AdvancedStorageSlotDefinition> slots, StorageType type)
    : IStorageDefinition
{
    public List<AdvancedStorageSlotDefinition> Slots { get; } = slots;

    public int Id { get; } = id;

    public string Name => TranslationManager.Get("structures.types." + Id);

    public StorageType Type { get; } = type;
}

// ReSharper disable once ClassNeverInstantiated.Global
public class AdvancedStorageSlotDefinition(Dictionary<int, int> restrictedItems, IReadOnlyList<int> slotOrder)
{
    public Dictionary<int, int> RestrictedItems { get; } = restrictedItems;
    public SlotOrder SlotOrder { get; } = new(slotOrder);
}

public class SlotOrder
{
    private readonly Dictionary<int, int> _itemIdToIndex = new();

    public SlotOrder(IReadOnlyList<int> slotOrder)
    {
        for (var i = 0; i < slotOrder.Count; i++)
        {
            _itemIdToIndex[slotOrder[i]] = i;
        }
    }

    public int? GetIndexForItem(int itemId)
    {
        return _itemIdToIndex.GetValueOrDefault(itemId);
    }

    public int GetMaxIndex()
    {
        return _itemIdToIndex.Count - 1;
    }
}