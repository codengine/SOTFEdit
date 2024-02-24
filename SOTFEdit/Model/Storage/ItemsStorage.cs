using System.Collections.Generic;
using System.Linq;
using SOTFEdit.Model.SaveData.Storage;
using SOTFEdit.Model.SaveData.Storage.Module;
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
        var supportedItems = new List<ItemWrapper>();
        if (Definition.RestrictedItemIds?.Count == 0)
        {
            return supportedItems;
        }

        var baseQ = itemList.Select(item => item.Value);

        if (Definition.RestrictedItemIds is { Count: > 0 } restrictedItemIds)
        {
            baseQ = baseQ.Where(item => restrictedItemIds.Contains(item.Id));
        }
        else
        {
            baseQ = baseQ.Where(item =>
                item is { IsInventoryItem: true } && (item.StorageMax?.Shelf > 0 || item.StorageMax?.Holder > 0));
        }

        foreach (var item in baseQ)
        {
            AddEffectiveSupportedItem(item, storageDefinition, supportedItems);
        }

        return supportedItems.OrderBy(item => item.Name).ToList();
    }

    protected override List<ItemWrapper> GetSupportedItems()
    {
        return _supportedItems;
    }

    public static StorageSaveData GenericToStorageSaveData(IStorageDefinition storageDefinition,
        IEnumerable<StorageSlot> slots)
    {
        var storageSaveData = new StorageSaveData
        {
            Id = storageDefinition.Id,
            Storages = new List<StorageBlock>()
        };

        foreach (var slot in slots)
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

                if (item.FoodSpoilStorageModuleWrapper is { } moduleWrapper)
                {
                    for (var i = 0; i < storedItem.Count; i++)
                    {
                        storageItemBlock.UniqueItems.Add(new UniqueItem
                        {
                            Modules = new List<IStorageModule>
                            {
                                moduleWrapper.FoodSpoilStorageModule
                            }
                        });
                    }
                }
                else if (storedItem.Modules?.Count > 0)
                {
                    for (var i = 0; i < storedItem.Count; i++)
                    {
                        storageItemBlock.UniqueItems.Add(new UniqueItem
                        {
                            Modules = storedItem.Modules
                        });
                    }
                }

                storageBlock.ItemBlocks.Add(storageItemBlock);
            }

            storageSaveData.Storages.Add(storageBlock);
        }

        return storageSaveData;
    }

    public override StorageSaveData ToStorageSaveData()
    {
        return GenericToStorageSaveData(Definition, Slots);
    }
}