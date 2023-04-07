using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SOTFEdit.Model.SaveData.Storage;
using SOTFEdit.Model.SaveData.Storage.Module;

namespace SOTFEdit.Model.Storage;

public class FoodStorage : RestrictedStorage
{
    private const int DefaultItemIdForUnselectedSlot = 436; //fish
    private const int DefaultVariantForUnselectedSlot = 4; //dried
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public FoodStorage(StorageDefinition definition, ItemList itemList, int index) : base(definition,
        itemList, index)
    {
    }

    public override void SetSaveData(StorageSaveData saveData)
    {
        var currentSlot = 0;

        Pos = saveData.Pos;

        var supportedItems = base.GetSupportedItems();

        foreach (var itemBlock in saveData.Storages.SelectMany(storageBlock => storageBlock.ItemBlocks))
        {
            if (supportedItems.FirstOrDefault(supportedItem => supportedItem.Item.Id == itemBlock.ItemId) is not
                { } item)
            {
                Logger.Warn($"Item {itemBlock.ItemId} is not in list of supported items, will skip");
                continue;
            }

            if (item.Item.Modules?.ToDictionary(itemModuleDefinition => itemModuleDefinition.ModuleId) is not
                { } itemModuleDefinitions)
            {
                Logger.Warn($"Item {itemBlock.ItemId} has no modules defined, will skip");
                continue;
            }

            foreach (var uniqueItem in itemBlock.UniqueItems)
            {
                if (currentSlot >= Slots.Count)
                {
                    var storageSlot = new StorageSlot();
                    Slots.Add(storageSlot);
                }

                if (uniqueItem.Modules.Count == 0)
                {
                    Slots[currentSlot++].StoredItems.Add(new StoredItem(null, 0, supportedItems,
                        Definition.MaxPerSlot));
                    continue;
                }

                foreach (var module in uniqueItem.Modules)
                {
                    if (!itemModuleDefinitions.ContainsKey(module.GetModuleId()))
                    {
                        throw new Exception(
                            $"Module definition {module.GetModuleId()} for item {itemBlock.ItemId} not found");
                    }

                    if (module is not FoodSpoilStorageModule foodSpoilStorageModule)
                    {
                        throw new Exception($"Unexpected module type: {module.GetType().Name}");
                    }

                    var selectedItem = supportedItems.FirstOrDefault(itemWrapper =>
                        itemWrapper.Item.Id == itemBlock.ItemId &&
                        (itemWrapper.ModuleWrapper?.Module.IsEqualTo(foodSpoilStorageModule) ?? false));

                    var storedItem = new StoredItem(selectedItem, selectedItem == null ? 0 : 1,
                        supportedItems, Definition.MaxPerSlot);
                    storedItem.PropertyChanged += OnStoredItemPropertyChanged;
                    Slots[currentSlot++].StoredItems.Add(storedItem);
                }
            }
        }

        foreach (var storageSlot in Slots)
        {
            if (storageSlot.StoredItems.Count != 0)
            {
                continue;
            }

            var storedItem = new StoredItem(null, 0, supportedItems,
                Definition.MaxPerSlot);
            storedItem.PropertyChanged += OnStoredItemPropertyChanged;
            storageSlot.StoredItems.Add(storedItem);
        }

        OnPropertyChanged(nameof(Description));
    }

    public override StorageSaveData ToStorageSaveData()
    {
        var storageSaveData = new StorageSaveData
        {
            Id = Definition.Id,
            Storages = new List<StorageBlock>()
        };

        var groupedByItemIds = Slots.SelectMany(slot => slot.StoredItems)
            .Where(item => item is { Count: > 0, SelectedItem: not null })
            .GroupBy(item => item.SelectedItem?.Item.Id ?? -1)
            .ToDictionary(items => items.Key, items => items.ToList());

        var storageBlock = new StorageBlock
        {
            ItemBlocks = new List<StorageItemBlock>(groupedByItemIds.Keys.Count)
        };
        storageSaveData.Storages.Add(storageBlock);

        foreach (var grouped in groupedByItemIds)
        {
            if (grouped.Key == -1)
            {
                continue;
            }

            var storageItemBlock = new StorageItemBlock
            {
                TotalCount = grouped.Value.Count,
                ItemId = grouped.Key,
                UniqueItems = new List<UniqueItem>(grouped.Value.Count)
            };

            foreach (var storedItem in grouped.Value)
            {
                if (storedItem.Count == 0 || storedItem.SelectedItem is not { } item)
                {
                    continue;
                }

                storageItemBlock.UniqueItems.Add(new UniqueItem
                {
                    Modules = new List<IStorageModule>
                    {
                        item.ModuleWrapper?.Module ?? new FoodSpoilStorageModule(3, 1)
                    }
                });
            }

            storageBlock.ItemBlocks.Add(storageItemBlock);
        }

        return storageSaveData;
    }

    public override void SetAllToMax()
    {
        foreach (var slot in Slots)
        foreach (var storedItem in slot.StoredItems)
            if (!storedItem.HasItem() && storedItem.SupportedItems.Count > 0)
            {
                storedItem.SelectedItem = storedItem.SupportedItems
                    .FirstOrDefault(wrapper =>
                        wrapper.Item.Id == DefaultItemIdForUnselectedSlot &&
                        wrapper.ModuleWrapper?.Module is FoodSpoilStorageModule
                        {
                            CurrentState: DefaultVariantForUnselectedSlot
                        });
            }
            else if (storedItem.HasItem())
            {
                storedItem.Count = storedItem.Max;
            }
    }
}