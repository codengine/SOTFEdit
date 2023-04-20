using System;
using System.Collections.Generic;
using SOTFEdit.Model.SaveData.Storage;
using SOTFEdit.Model.SaveData.Storage.Module;

namespace SOTFEdit.Model.Storage;

public class StorageWithModulePerItem : RestrictedStorage
{
    private readonly Func<IStorageModule>? _moduleFactory;

    public StorageWithModulePerItem(StorageDefinition definition, ItemList itemList, int index,
        Func<IStorageModule>? moduleFactory = null) : base(definition,
        itemList, index)
    {
        _moduleFactory = moduleFactory;
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
                    ItemId = item.Item.Id,
                    UniqueItems = new List<UniqueItem>(storedItem.Count)
                };

                if (_moduleFactory != null)
                {
                    for (var i = 0; i < storedItem.Count; i++)
                        storageItemBlock.UniqueItems.Add(new UniqueItem
                        {
                            Modules = new List<IStorageModule>
                            {
                                _moduleFactory.Invoke()
                            }
                        });                    
                }

                storageBlock.ItemBlocks.Add(storageItemBlock);
            }

            storageSaveData.Storages.Add(storageBlock);
        }

        foreach (var storageSlot in Slots)
            if (storageSlot.StoredItems.Count == 0)
            {
                storageSlot.StoredItems.Add(new StoredItem(null, 0, GetSupportedItems(), Definition.MaxPerSlot));
            }

        return storageSaveData;
    }
}