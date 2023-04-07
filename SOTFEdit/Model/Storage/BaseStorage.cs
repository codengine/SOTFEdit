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
using SOTFEdit.Model.SaveData.Storage.Module;
using SOTFEdit.View.Storage;

namespace SOTFEdit.Model.Storage;

public abstract partial class BaseStorage : ObservableObject, IStorage
{
    private readonly int _index;
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    protected readonly StorageDefinition Definition;

    [ObservableProperty] private Position? _pos;

    protected BaseStorage(StorageDefinition definition, int index)
    {
        Definition = definition;
        _index = index;

        for (var i = 0; i < definition.Slots; i++)
        {
            var storageSlot = new StorageSlot();
            Slots.Add(storageSlot);
        }
    }

    public ObservableCollection<StorageSlot> Slots { get; } = new();

    public List<ItemWrapper> SupportedItems => GetSupportedItems();

    public string Description => $"{Definition.Name} #{_index} ({GetTotalStored()})";

    public virtual void SetSaveData(StorageSaveData saveData)
    {
        var currentSlot = 0;

        Pos = saveData.Pos;

        var supportedItems = GetSupportedItems();

        foreach (var storageBlock in saveData.Storages)
        {
            if (currentSlot >= Slots.Count)
            {
                var storageSlot = new StorageSlot();
                Slots.Add(storageSlot);
            }

            if (storageBlock.ItemBlocks.Count == 0)
            {
                currentSlot++;
                continue;
            }

            foreach (var itemBlock in storageBlock.ItemBlocks)
            {
                var candidates = supportedItems.Where(supportedItem => supportedItem.Item.Id == itemBlock.ItemId)
                    .ToList();

                if (candidates.Count == 0)
                {
                    _logger.Warn(
                        $"Item {itemBlock.ItemId} is not in list of supported items for {Definition.Name} ({Definition.Id}), will skip");
                    continue;
                }

                ItemWrapper? foundCandidate = null;

                if (candidates.Count == 1 || itemBlock.UniqueItems.Count == 0)
                {
                    foundCandidate = candidates[0];
                }
                else
                {
                    foreach (var candidate in candidates)
                    {
                        if (candidate.ModuleWrapper is not { } moduleWrapper ||
                            !itemBlock.FirstModuleIsEqualTo(moduleWrapper.Module))
                        {
                            continue;
                        }

                        foundCandidate = candidate;
                        break;
                    }
                }

                if (foundCandidate != null)
                {
                    var storedItem = new StoredItem(foundCandidate, itemBlock.TotalCount, supportedItems,
                        Definition.MaxPerSlot);
                    storedItem.PropertyChanged += OnStoredItemPropertyChanged;
                    Slots[currentSlot++].StoredItems.Add(storedItem);
                }
                else
                {
                    _logger.Warn(
                        $"Item {itemBlock.ItemId} - no candidate found for {Definition.Name} ({Definition.Id}), will skip");
                }
            }
        }

        RemoveOverflowingSlots();

        foreach (var storageSlot in Slots)
        {
            if (storageSlot.StoredItems.Count > 0)
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

    public virtual void SetAllToMax()
    {
        foreach (var slot in Slots)
        foreach (var storedItem in slot.StoredItems)
            if (!storedItem.HasItem() && storedItem.SupportedItems.Count == 1)
            {
                storedItem.SelectedItem = storedItem.SupportedItems[0];
            }
            else if (storedItem.HasItem())
            {
                storedItem.Count = storedItem.Max;
            }
    }

    private void RemoveOverflowingSlots()
    {
        if (Slots.Count > Definition.Slots)
        {
            for (var i = Slots.Count - 1; i >= 0; i--)
            {
                var slot = Slots[i];
                if (slot.StoredItems.Count == 0)
                {
                    Slots.RemoveAt(i);
                }

                if (Slots.Count == Definition.Slots)
                {
                    break;
                }
            }
        }

        if (Slots.Count <= Definition.Slots)
        {
            return;
        }

        for (var i = Slots.Count - 1; i >= 0; i--)
        {
            Slots.RemoveAt(i);

            if (Slots.Count == Definition.Slots)
            {
                break;
            }
        }
    }

    [RelayCommand]
    private void OpenMapAtStoragePos()
    {
        if (Pos != null)
        {
            WeakReferenceMessenger.Default.Send(new ZoomToPosEvent(Pos));
        }
    }

    private int GetTotalStored()
    {
        return Slots
            .Select(item => item.GetTotalStored())
            .Sum();
    }

    protected abstract List<ItemWrapper> GetSupportedItems();

    protected void OnStoredItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Count")
        {
            OnPropertyChanged(nameof(Description));
        }
    }

    protected void AddEffectiveSupportedItem(Item item, StorageDefinition storageDefinition, List<ItemWrapper> target)
    {
        if (item.Modules?.Count > 0)
        {
            target.AddRange(
                from itemModule in item.Modules
                from variant in itemModule.Variants
                select new ItemWrapper(item, storageDefinition.MaxPerSlot, BuildModuleWrapper(itemModule, variant))
            );
        }
        else
        {
            target.Add(new ItemWrapper(item, storageDefinition.MaxPerSlot));
        }
    }


    private ModuleWrapper? BuildModuleWrapper(ItemModule itemModule, ItemVariant variant)
    {
        if (FoodSpoilStorageModule.SupportsModuleId(itemModule.ModuleId))
        {
            var storageModule = new FoodSpoilStorageModule(itemModule.ModuleId, variant.State);
            return new ModuleWrapper(storageModule, variant.Name);
        }

        _logger.Warn($"Unsupported moduleId: {itemModule.ModuleId}");

        return null;
    }
}