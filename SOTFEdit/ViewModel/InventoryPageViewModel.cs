using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Inventory;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public partial class InventoryPageViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly HashSet<int> _blueprintItemIds = new()
    {
        665, //Item Plating
        666, //Powered Cross
        670, //Gore Chair
        671, //Gore Couch
        672, //Uber Trap
        679, //Grind Trap
        680, //Spotlight
        681, //Clock
        682, //Spin Trap
        688, //Spear Thrower Trap
        701, //Leg Lamp
        709, //Teleporter
        710, //Repel Shrine
        711, //Attract Shrine
        713, //Plater Counter
        714, //Gold Armor Plater
        726 //Hang Glider Launcher
    };

    private readonly ObservableCollectionEx<InventoryItem> _inventory = new();

    private readonly DispatcherTimer _inventoryFilterTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(300)
    };

    private readonly ItemList _itemList;
    private readonly ObservableCollectionEx<InventoryItem> _unassignedItems = new();

    private readonly DispatcherTimer _unassignedItemsFilterTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(300)
    };

    [ObservableProperty]
    private string _inventoryFilter = "";

    private string _normalizedInventoryFilter = "";
    private string _normalizedUnassignedItemsFilter = "";

    [ObservableProperty]
    private string _unassignedItemsFilter = "";


    public InventoryPageViewModel(GameData gameData)
    {
        _inventoryFilterTimer.Tick += OnInventoryFilterTimerTick;
        _unassignedItemsFilterTimer.Tick += OnUnassignedItemsFilterTimerTick;

        var inventorySortDescription = new SortDescription("Name", ListSortDirection.Ascending);

        InventoryCollectionView = CollectionViewSource.GetDefaultView(_inventory);
        InventoryCollectionView.Filter = OnFilterInventory;
        InventoryCollectionView.SortDescriptions.Add(inventorySortDescription);

        UnassignedItemsCollectionView = CollectionViewSource.GetDefaultView(_unassignedItems);
        UnassignedItemsCollectionView.Filter = OnFilterUnassignedItems;
        UnassignedItemsCollectionView.SortDescriptions.Add(inventorySortDescription);

        _itemList = gameData.Items;

        var categories = gameData.Items
            .Where(item => item.Value.IsInventoryItem)
            .Select(item => item.Value.Type)
            .Distinct()
            .OrderBy(type => type)
            .Select(type => new Category(type));

        Categories.AddRange(categories);

        _inventory.CollectionChanged += (_, _) =>
        {
            RemoveAllEquippedCommand.NotifyCanExecuteChanged();
            SetAllEquippedToMaxCommand.NotifyCanExecuteChanged();
            SetAllEquippedToMinCommand.NotifyCanExecuteChanged();
        };

        _unassignedItems.CollectionChanged += (_, _) => { AddAllFromCategoryCommand.NotifyCanExecuteChanged(); };

        SetupListeners();
    }

    public ObservableCollectionEx<Category> Categories { get; } = new();

    public ICollectionView InventoryCollectionView { get; }
    public ICollectionView UnassignedItemsCollectionView { get; }

    private void OnInventoryFilterTimerTick(object? sender, EventArgs e)
    {
        _inventoryFilterTimer.Stop();
        _normalizedInventoryFilter = TranslationHelper.Normalize(_inventoryFilter).ToLower();
        InventoryCollectionView.Refresh();
    }

    private void OnUnassignedItemsFilterTimerTick(object? sender, EventArgs e)
    {
        _unassignedItemsFilterTimer.Stop();
        _normalizedUnassignedItemsFilter = TranslationHelper.Normalize(_unassignedItemsFilter).ToLower();
        UnassignedItemsCollectionView.Refresh();
    }

    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnInventoryFilterChanged(string value)
    {
        _inventoryFilterTimer.Stop();
        _inventoryFilterTimer.Start();
    }

    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnUnassignedItemsFilterChanged(string value)
    {
        _unassignedItemsFilterTimer.Stop();
        _unassignedItemsFilterTimer.Start();
    }

    [RelayCommand]
    private void RemoveItemFromInventory(InventoryItem? inventoryItem)
    {
        if (inventoryItem == null)
        {
            return;
        }

        _unassignedItems.Add(inventoryItem);
        _inventory.Remove(inventoryItem);
    }

    [RelayCommand]
    private void AddUnassignedItem(InventoryItem? inventoryItem)
    {
        if (inventoryItem == null)
        {
            return;
        }

        _inventory.Add(inventoryItem);
        _unassignedItems.Remove(inventoryItem);
    }

    private bool OnFilterUnassignedItems(object? obj)
    {
        var filter = _normalizedUnassignedItemsFilter;
        if (string.IsNullOrWhiteSpace(filter) || obj == null)
        {
            return true;
        }

        var item = (InventoryItem)obj;
        return FilterItem(item, filter);
    }

    private bool OnFilterInventory(object? obj)
    {
        var filter = _normalizedInventoryFilter;
        if (string.IsNullOrWhiteSpace(filter) || obj == null)
        {
            return true;
        }

        var item = (InventoryItem)obj;
        return FilterItem(item, filter);
    }

    private static bool FilterItem(InventoryItem item, string filter)
    {
        return item.NormalizedName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
               item.TypeRendered.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
               item.Id.ToString().Contains(filter);
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => OnSelectedSavegameChanged(m));
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent m)
    {
        if (m.SelectedSavegame == null)
        {
            _inventory.Clear();
            _unassignedItems.Clear();
            return;
        }

        HashSet<int> assignedItems = new();

        var saveData =
            m.SelectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType
                .PlayerInventorySaveData)?.Parent.ToObject<PlayerInventoryDataModel>();
        if (saveData != null)
        {
            var playerInventoryModel = saveData.Data.PlayerInventory;
            var inventoryItems = playerInventoryModel.ItemInstanceManagerData.ItemBlocks
                .Select(itemBlock =>
                {
                    var item = _itemList.GetItem(itemBlock.ItemId);
                    if (item?.StorageMax?.Inventory is { } maxInInventory && maxInInventory < itemBlock.TotalCount)
                    {
                        item = (Item)item.Clone();
                        item.StorageMax = new StorageMax(itemBlock.TotalCount, item.StorageMax?.Shelf ?? 0,
                            item.StorageMax?.Holder);
                        Logger.Info(
                            $"Defined max in inventory for {item.Id} is lower ({maxInInventory}) than in savedata ({itemBlock.TotalCount})");
                    }

                    return new InventoryItem(itemBlock, item);
                })
                .ToList();

            var equippedItems = playerInventoryModel.EquippedItems?.Select(itemBlock =>
                {
                    var item = _itemList.GetItem(itemBlock.ItemId);
                    if (item?.StorageMax?.Inventory is { } maxInInventory && maxInInventory < itemBlock.TotalCount)
                    {
                        Logger.Info(
                            $"Defined max in inventory for {item.Id} is lower ({maxInInventory}) than in savedata ({itemBlock.TotalCount})");
                    }

                    itemBlock.TotalCount = 1;

                    return new InventoryItem(itemBlock, item, true);
                })
                .ToList();

            if (equippedItems != null)
            {
                inventoryItems.AddRange(equippedItems);
            }

            _inventory.ReplaceRange(inventoryItems);

            foreach (var inventoryItem in inventoryItems)
            {
                assignedItems.Add(inventoryItem.Id);
            }
        }

        var unassignedItems = _itemList.Where(item => !assignedItems.Contains(item.Value.Id))
            .Where(item => item.Value is { IsInventoryItem: true, StorageMax.Inventory: > 0 })
            .Select(item => BuildUnassignedItem(item.Value));

        _unassignedItems.ReplaceRange(unassignedItems);
    }

    private static InventoryItem BuildUnassignedItem(Item item)
    {
        var itemBlock = ItemBlockModel.Unassigned(item);
        return new InventoryItem(itemBlock, item);
    }

    public bool Update(Savegame savegame)
    {
        var saveDataWrapper = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerInventorySaveData);

        if (saveDataWrapper == null)
        {
            return false;
        }

        var selectedItems = new List<InventoryItem>(_inventory);

        var hasChanged = PlayerInventoryDataModel.Merge(saveDataWrapper, selectedItems);
        return EnsureBlueprintStatusUpdated(savegame.SavegameStore, selectedItems) || hasChanged;
    }

    private bool EnsureBlueprintStatusUpdated(SavegameStore savegameStore, List<InventoryItem> inventoryItems)
    {
        if (savegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerStateSaveData) is not { } saveDataWrapper)
        {
            return false;
        }

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.PlayerState) is not { } playerState ||
            playerState["_entries"] is not JArray entries)
        {
            return false;
        }

        var addedBlueprintItems = new HashSet<int>();

        var hasChanges = false;

        foreach (var inventoryItem in
                 inventoryItems.Where(inventoryItem => _blueprintItemIds.Contains(inventoryItem.Id)))
        {
            addedBlueprintItems.Add(inventoryItem.Id);
        }

        var blueprintTokensExisting = new HashSet<int>();

        foreach (var jToken in entries)
        {
            var name = jToken["Name"]?.Value<string>();

            foreach (var itemId in _blueprintItemIds.Where(itemId => name == $"DiscoverablePageUnlocked_{itemId}"))
            {
                blueprintTokensExisting.Add(itemId);

                var isEnabled = jToken["BoolValue"]?.Value<bool>() ?? false;
                var isAdded = addedBlueprintItems.Contains(itemId);
                if (isEnabled != isAdded)
                {
                    jToken["BoolValue"] = isAdded;
                    hasChanges = true;
                }

                break;
            }
        }

        foreach (var blueprintItemId in _blueprintItemIds)
        {
            if (blueprintTokensExisting.Contains(blueprintItemId) || !addedBlueprintItems.Contains(blueprintItemId))
            {
                continue;
            }

            entries.Add(new JObject
            {
                { "Name", $"DiscoverablePageUnlocked_{blueprintItemId}" },
                { "BoolValue", true }
            });
            hasChanges = true;
        }

        if (hasChanges)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.PlayerState);
        }

        return hasChanges;
    }

    [RelayCommand(CanExecute = nameof(HasInventoryItems))]
    private void SetAllEquippedToMin()
    {
        foreach (var inventoryItem in _inventory)
        {
            inventoryItem.TotalCount = 1;
        }
    }

    [RelayCommand(CanExecute = nameof(HasInventoryItems))]
    private void SetAllEquippedToMax()
    {
        foreach (var inventoryItem in _inventory)
        {
            inventoryItem.TotalCount = inventoryItem.Max;
        }
    }

    [RelayCommand(CanExecute = nameof(HasInventoryItems))]
    private void RemoveAllEquipped()
    {
        foreach (var inventoryItem in _inventory.ToList())
        {
            RemoveItemFromInventory(inventoryItem);
        }
    }

    private bool HasInventoryItems()
    {
        return _inventory.Count > 0;
    }

    private bool HasUnassignedItems()
    {
        return _unassignedItems.Count > 0;
    }

    [RelayCommand(CanExecute = nameof(HasUnassignedItems))]
    private void AddAllFromCategory(Category category)
    {
        var items = _unassignedItems.Where(item => item.Type == category.Type).ToList();
        foreach (var inventoryItem in items)
        {
            _unassignedItems.Remove(inventoryItem);
        }

        _inventory.AddRange(items);
    }
}

public class Category
{
    public Category(string type)
    {
        Type = type;
    }

    public string Type { get; }

    // ReSharper disable once UnusedMember.Global
    public string TypeRendered => TranslationManager.Get("itemTypes." + Type);
}