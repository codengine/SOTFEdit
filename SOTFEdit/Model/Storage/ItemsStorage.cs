using System.Collections.Generic;
using System.Linq;
using SOTFEdit.Model.SaveData.Storage;
using SOTFEdit.View.Storage;

namespace SOTFEdit.Model.Storage;

public class ItemsStorage : BaseStorage
{
    private readonly List<ItemWrapper> _supportedItems;

    public ItemsStorage(StorageDefinition definition, ItemList itemList, int index) : base(definition,
        index)
    {
        _supportedItems = GetSupportedItems(itemList, definition);
    }

    private List<ItemWrapper> GetSupportedItems(ItemList itemList, StorageDefinition storageDefinition)
    {
        if (Definition.RestrictedItemIds?.Count == 0)
        {
            return new List<ItemWrapper>();
        }

        var baseQ = itemList.Select(item => item.Value)
            .Where(item => item is { IsInventoryItem: true, StorageMax.Shelf: > 0 });

        if (Definition.RestrictedItemIds is { Count: > 0 } restrictedItemIds)
        {
            baseQ = baseQ.Where(item => restrictedItemIds.Contains(item.Id));
        }

        return baseQ.Select(item => new ItemWrapper(item, storageDefinition.MaxPerSlot)).ToList();
    }

    protected override List<ItemWrapper> GetSupportedItems()
    {
        return _supportedItems;
    }

    public override StorageSaveData ToStorageSaveData()
    {
        var storageSaveData = new StorageSaveData
        {
            Id = Definition.Id,
            Storages = new List<StorageBlock>()
        };

        foreach (var slot in Slots)
        {
            var storageBlock = new StorageBlock
            {
                ItemBlocks = new List<StorageItemBlock>(slot.StoredItems.Count)
            };

            foreach (var storedItem in slot.StoredItems)
            {
                if (storedItem.Count == 0 || storedItem.SelectedItem is not { } item)
                {
                    continue;
                }

                var storageItemBlock = new StorageItemBlock
                {
                    TotalCount = storedItem.Count,
                    ItemId = item.Item.Id
                };

                storageBlock.ItemBlocks.Add(storageItemBlock);
            }

            storageSaveData.Storages.Add(storageBlock);
        }

        return storageSaveData;
    }
}