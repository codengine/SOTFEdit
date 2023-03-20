using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
using SOTFEdit.Model.SaveData.Storage;
using SOTFEdit.View.Storage;

namespace SOTFEdit.Model.Storage;

public abstract class BaseStorage : ObservableObject, IStorage
{
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly int _index;

    protected readonly StorageDefinition Definition;

    protected BaseStorage(StorageDefinition definition, int index)
    {
        Definition = definition;
        _index = index;
        for (var i = 0; i < definition.Slots; i++)
        {
            var storageSlot = new StorageSlot(i);
            Slots.Add(storageSlot);
        }
    }

    public ObservableCollection<StorageSlot> Slots { get; } = new();

    public string Description => $"{Definition.Name} #{_index} ({GetTotalStored()})";

    public List<ItemWrapper> SupportedItems => GetSupportedItems();

    public virtual void SetItemsFromJson(StorageSaveData saveData)
    {
        var currentSlot = 0;

        var supportedItems = GetSupportedItems();

        foreach (var storageBlock in saveData.Storages)
        {
            if (currentSlot >= Slots.Count)
            {
                Slots.Add(new StorageSlot(currentSlot));
            }

            if (storageBlock.ItemBlocks.Count == 0)
            {
                currentSlot++;
                continue;
            }

            foreach (var itemBlock in storageBlock.ItemBlocks)
            {
                if (supportedItems.FirstOrDefault(supportedItem => supportedItem.Item.Id == itemBlock.ItemId) is not
                    { } item)
                {
                    _logger.Warn(
                        $"Item {itemBlock.ItemId} is not in list of supported items for {Definition.Name} ({Definition.Id}), will skip");
                    continue;
                }

                var storedItem = new StoredItem(item, itemBlock.TotalCount, supportedItems,
                    Definition.MaxPerSlot);
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

            var storedItem = new StoredItem(null, 0, supportedItems, Definition.MaxPerSlot);
            storedItem.PropertyChanged += OnStoredItemPropertyChanged;
            storageSlot.StoredItems.Add(storedItem);
        }

        OnPropertyChanged(nameof(Description));
    }

    public abstract StorageSaveData ToStorageSaveData();

    private int GetTotalStored()
    {
        return Slots
            .Select(item => item.GetTotalStored())
            .Sum();
    }

    public abstract List<ItemWrapper> GetSupportedItems();

    protected void OnStoredItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Count")
        {
            OnPropertyChanged(nameof(Description));
        }
    }
}