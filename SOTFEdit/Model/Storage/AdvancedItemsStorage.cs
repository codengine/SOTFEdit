using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Storage;
using SOTFEdit.View.Storage;

namespace SOTFEdit.Model.Storage;

public partial class AdvancedItemsStorage : ObservableObject, IStorage
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly AdvancedStorageDefinition _definition;
    private readonly int _index;
    private readonly ItemList _items;

    [ObservableProperty] private Position? _pos;

    public AdvancedItemsStorage(AdvancedStorageDefinition definition, ItemList items, int index)
    {
        _definition = definition;
        _items = items;
        _index = index;

        for (var i = 0; i < definition.Slots.Count; i++)
        {
            var storageSlot = new StorageSlot();
            Slots.Add(storageSlot);
        }
    }

    public ObservableCollection<StorageSlot> Slots { get; } = new();

    public List<ItemWrapper> SupportedItems => GetSupportedItems();

    public string Description => $"{_definition.Name} #{_index} ({GetTotalStored()})";

    public void SetSaveData(StorageSaveData saveData)
    {
        Pos = saveData.Pos;
        var currentSlot = 0;
        var storedItems = new Dictionary<int, StoredItem>();

        foreach (var itemBlock in saveData.Storages.Select(storageBlock => storageBlock.ItemBlocks.FirstOrDefault()))
        {
            if (itemBlock == null)
            {
                currentSlot++;
                continue;
            }

            if (GetSlotIndex(currentSlot) is not { } slotIndex)
            {
                break;
            }

            var supportedItems = GetSupportedItems(slotIndex);
            var candidate = supportedItems.FirstOrDefault(item => item.Item.Id == itemBlock.ItemId);

            if (candidate == null)
            {
                Logger.Warn(
                    $"Item {itemBlock.ItemId} is not in list of supported items for {_definition.Name} ({_definition.Id}), will skip");
                continue;
            }

            var storedItem = new StoredItem(candidate, itemBlock.TotalCount, supportedItems, null);
            storedItem.PropertyChanged += OnStoredItemPropertyChanged;
            if (!storedItems.TryAdd(slotIndex, storedItem))
            {
                Logger.Warn($"Found more than one item for slotIndex {slotIndex}");
                continue;
            }

            currentSlot++;
        }

        for (var i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];
            slot.StoredItems.Clear();
            if (storedItems.GetValueOrDefault(i) is { } storedItem)
            {
                slot.StoredItems.Add(storedItem);
            }
            else
            {
                var supportedItems = GetSupportedItems(i);
                var emptyStoredItem = new StoredItem(null, 0, supportedItems, null);
                emptyStoredItem.PropertyChanged += OnStoredItemPropertyChanged;
                slot.StoredItems.Add(emptyStoredItem);
            }
        }

        OnPropertyChanged(nameof(Description));
    }

    public StorageSaveData ToStorageSaveData()
    {
        var storageSaveData = new StorageSaveData
        {
            Id = _definition.Id,
            Storages = new List<StorageBlock>(),
            Pos = Pos
        };

        for (var i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];
            var slotDefinition = _definition.Slots[i];
            if (slot.StoredItems.FirstOrDefault() is not { SelectedItem: { } selectedItem } storedItem ||
                storedItem.Count == 0 || slotDefinition.SlotOrder.GetIndexForItem(selectedItem.Item.Id) is not
                    { } slotOrder)
            {
                for (var j = 0; j <= slotDefinition.SlotOrder.GetMaxIndex(); j++)
                {
                    storageSaveData.Storages.Add(new StorageBlock());
                }

                continue;
            }

            for (var j = 0; j <= slotDefinition.SlotOrder.GetMaxIndex(); j++)
            {
                var storageBlock = new StorageBlock();

                if (slotOrder == j)
                {
                    var storageItemBlock = new StorageItemBlock
                    {
                        ItemId = selectedItem.Item.Id,
                        TotalCount = storedItem.Count
                    };
                    storageBlock.ItemBlocks.Add(storageItemBlock);
                }

                storageSaveData.Storages.Add(storageBlock);
            }
        }

        return storageSaveData;
    }

    public void SetAllToMax()
    {
        foreach (var slot in Slots)
        foreach (var storedItem in slot.StoredItems)
        {
            if (!storedItem.HasItem() && storedItem.SupportedItems.Count == 1)
            {
                storedItem.SelectedItem = storedItem.SupportedItems[0];
            }
            else if (storedItem.HasItem())
            {
                storedItem.Count = storedItem.Max;
            }
        }
    }

    public int GetStorageTypeId()
    {
        return _definition.Id;
    }

    public void ApplyFrom(IStorage storage)
    {
        SetSaveData(storage.ToStorageSaveData());
    }

    private List<ItemWrapper> GetSupportedItems()
    {
        var itemIds = new HashSet<int>();
        var result = new List<ItemWrapper>();

        foreach (var (itemId, max) in _definition.Slots.SelectMany(slot => slot.RestrictedItems))
        {
            if (!itemIds.Add(itemId))
            {
                continue;
            }

            var item = _items.GetItem(itemId);
            if (item != null)
            {
                result.Add(new ItemWrapper(item, max));
            }
        }

        return result;
    }

    private void OnStoredItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Count")
        {
            OnPropertyChanged(nameof(Description));
        }
    }

    private List<ItemWrapper> GetSupportedItems(int slotIndex)
    {
        var itemWrappers = new List<ItemWrapper>();

        if (slotIndex >= _definition.Slots.Count)
        {
            Logger.Warn(
                $"Found more slots than expected for {_definition.Name} ({_definition.Id}), will skip remaining ones");
            return itemWrappers;
        }

        var slotDefinition = _definition.Slots[slotIndex];

        foreach (var (itemId, max) in slotDefinition.RestrictedItems)
        {
            var item = _items.GetItem(itemId);
            if (item == null)
            {
                continue;
            }

            itemWrappers.Add(new ItemWrapper(item, max));
        }

        return itemWrappers;
    }

    private int? GetSlotIndex(int currentSlot)
    {
        if (_definition.Id != 58) //58 = Advanced Log Sled, fix for Rabbit Holders
        {
            return currentSlot;
        }

        if (currentSlot <= 10)
        {
            return (currentSlot - 1) / 2;
        }

        return null;
    }

    private int GetTotalStored()
    {
        return Slots
            .Select(item => item.GetTotalStored())
            .Sum();
    }

    [RelayCommand]
    private void OpenMapAtStoragePos()
    {
        if (Pos != null)
        {
            WeakReferenceMessenger.Default.Send(new ZoomToPosEvent(Pos));
        }
    }
}