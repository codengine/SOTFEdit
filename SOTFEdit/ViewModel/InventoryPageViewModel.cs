using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Inventory;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public partial class InventoryPageViewModel : ObservableObject
{
    private readonly WpfObservableRangeCollection<InventoryItem> _inventory = new();

    private readonly DispatcherTimer _inventoryFilterTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(300)
    };

    private readonly ItemList _itemList;
    private readonly WpfObservableRangeCollection<InventoryItem> _unassignedItems = new();

    private readonly DispatcherTimer _unassignedItemsFilterTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(300)
    };

    [ObservableProperty] private string _inventoryFilter = "";
    private string _normalizedInventoryFilter = "";
    private string _normalizedUnassignedItemsFilter = "";
    [ObservableProperty] private string _unassignedItemsFilter = "";


    public InventoryPageViewModel(GameData gameData)
    {
        _inventoryFilterTimer.Tick += OnInventoryFilterTimerTick;
        _unassignedItemsFilterTimer.Tick += OnUnassignedItemsFilterTimerTick;

        InventoryCollectionView = new ListCollectionView(_inventory)
        {
            Filter = OnFilterInventory,
            SortDescriptions =
            {
                new SortDescription("Name", ListSortDirection.Ascending)
            }
        };
        UnassignedItemsCollectionView = new ListCollectionView(_unassignedItems)
        {
            Filter = OnFilterUnassignedItems,
            SortDescriptions =
            {
                new SortDescription("Name", ListSortDirection.Ascending)
            }
        };
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

    public WpfObservableRangeCollection<Category> Categories { get; } = new();

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
            (_, m) => { OnSelectedSavegameChanged(m); });
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
            var inventoryItems = saveData.Data.PlayerInventory.ItemInstanceManagerData.ItemBlocks
                .Select(itemBlock => new InventoryItem(itemBlock, _itemList.GetItem(itemBlock.ItemId)))
                .ToList();
            _inventory.ReplaceRange(inventoryItems);

            foreach (var inventoryItem in inventoryItems) assignedItems.Add(inventoryItem.Id);
        }

        var unassignedItems = _itemList.Where(item => !assignedItems.Contains(item.Value.Id))
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

        return PlayerInventoryDataModel.Merge(saveDataWrapper, selectedItems);
    }

    [RelayCommand(CanExecute = nameof(HasInventoryItems))]
    private void SetAllEquippedToMin()
    {
        foreach (var inventoryItem in _inventory) inventoryItem.TotalCount = 1;
    }

    [RelayCommand(CanExecute = nameof(HasInventoryItems))]
    private void SetAllEquippedToMax()
    {
        foreach (var inventoryItem in _inventory) inventoryItem.TotalCount = inventoryItem.Max;
    }

    [RelayCommand(CanExecute = nameof(HasInventoryItems))]
    private void RemoveAllEquipped()
    {
        foreach (var inventoryItem in _inventory.ToList()) RemoveItemFromInventory(inventoryItem);
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
        _unassignedItems.RemoveRange(items);
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