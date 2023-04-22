using System.Collections.Generic;
using System.Linq;
using SOTFEdit.View.Storage;

namespace SOTFEdit.Model.Storage;

public abstract class RestrictedStorage : BaseStorage
{
    private readonly List<ItemWrapper> _supportedItems;

    protected RestrictedStorage(StorageDefinition definition, ItemList itemList, int index) : base(
        definition, index)
    {
        _supportedItems = GetSupportedItems(itemList, definition);
    }

    private List<ItemWrapper> GetSupportedItems(ItemList itemList, StorageDefinition storageDefinition)
    {
        var supportedItems = new List<ItemWrapper>();

        var baseQ = itemList.Select(item => item.Value);

        if (Definition.RestrictedItemIds is { Count: > 0 } restrictedItemIds)
        {
            baseQ = baseQ.Where(item => restrictedItemIds.Contains(item.Id));
        }

        foreach (var item in baseQ)
        {
            AddEffectiveSupportedItem(item, storageDefinition, supportedItems);
        }

        return supportedItems;
    }


    protected override List<ItemWrapper> GetSupportedItems()
    {
        return _supportedItems;
    }
}