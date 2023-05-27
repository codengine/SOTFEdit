using System.Collections.Generic;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Storage;

// ReSharper disable once ClassNeverInstantiated.Global
public class AdvancedStorageDefinition : IStorageDefinition
{
    public AdvancedStorageDefinition(int id, List<AdvancedStorageSlotDefinition> slots, StorageType type)
    {
        Id = id;
        Slots = slots;
        Type = type;
    }

    public List<AdvancedStorageSlotDefinition> Slots { get; }

    public int Id { get; }

    public string Name => TranslationManager.Get("structures.types." + Id);

    public StorageType Type { get; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class AdvancedStorageSlotDefinition
{
    public AdvancedStorageSlotDefinition(Dictionary<int, int> restrictedItems, IReadOnlyList<int> slotOrder)
    {
        RestrictedItems = restrictedItems;
        SlotOrder = new SlotOrder(slotOrder);
    }

    public Dictionary<int, int> RestrictedItems { get; }
    public SlotOrder SlotOrder { get; }
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