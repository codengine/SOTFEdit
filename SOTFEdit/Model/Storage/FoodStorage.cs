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
            if (supportedItems.FirstOrDefault(supportedItem => supportedItem.Item.Id == itemBlock.ItemId) is null)
            {
                Logger.Warn($"Item {itemBlock.ItemId} is not in list of supported items, will skip");
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

                var foodSpoilStorageModule = uniqueItem.Modules
                    .OfType<FoodSpoilStorageModule>()
                    .FirstOrDefault();

                var selectedItem = supportedItems.FirstOrDefault(itemWrapper =>
                    itemWrapper.Item.Id == itemBlock.ItemId &&
                    foodSpoilStorageModule != null &&
                    (itemWrapper.FoodSpoilStorageModuleWrapper?.FoodSpoilStorageModule
                        .IsEqualTo(foodSpoilStorageModule) ?? false));

                var storedItem = new StoredItem(selectedItem, selectedItem == null ? 0 : 1,
                    supportedItems, Definition.MaxPerSlot, uniqueItem.Modules);
                storedItem.PropertyChanged += OnStoredItemPropertyChanged;
                Slots[currentSlot++].StoredItems.Add(storedItem);
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

                var modules = storedItem.Modules == null
                    ? new List<IStorageModule>()
                    : new List<IStorageModule>(storedItem.Modules);

                UpdateModules(item.Item, modules);

                storageItemBlock.UniqueItems.Add(new UniqueItem
                {
                    Modules = modules
                });
            }

            storageBlock.ItemBlocks.Add(storageItemBlock);
        }

        return storageSaveData;
    }

    private static void UpdateModules(Item item, ICollection<IStorageModule> modules)
    {
        if (item.FoodSpoilModuleDefinition is { } foodSpoilModuleDefinition &&
            !modules.OfType<FoodSpoilStorageModule>().Any())
        {
            modules.Add(foodSpoilModuleDefinition.BuildNewModuleWithDefaults());
        }

        if (item.SourceActorModuleDefinition is { } sourceActorModuleDefinition &&
            !modules.OfType<SourceActorStorageModule>().Any())
        {
            modules.Add(sourceActorModuleDefinition.BuildNewModuleWithDefaults());
        }
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
                        wrapper.FoodSpoilStorageModuleWrapper?.FoodSpoilStorageModule is
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