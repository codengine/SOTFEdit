using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Inventory;

namespace SOTFEdit.ViewModel;

public partial class InventoryPageViewModel : ObservableObject
{
    private readonly WpfObservableRangeCollection<InventoryItem> _inventory = new();
    private readonly ItemList _itemList;
    private readonly WpfObservableRangeCollection<InventoryItem> _unassignedItems = new();
    [ObservableProperty] private string _inventoryFilter = "";
    [ObservableProperty] private string _unassignedItemsFilter = "";

    public InventoryPageViewModel(GameData gameData)
    {
        InventoryCollectionView =
            new GenericCollectionView<InventoryItem>(
                (ListCollectionView)CollectionViewSource.GetDefaultView(_inventory))
            {
                Filter = OnFilterInventory
            };
        UnassignedItemsCollectionView =
            new GenericCollectionView<InventoryItem>(
                (ListCollectionView)CollectionViewSource.GetDefaultView(_unassignedItems))
            {
                Filter = OnFilterUnassignedItems
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

    public GenericCollectionView<InventoryItem> InventoryCollectionView { get; }
    public GenericCollectionView<InventoryItem> UnassignedItemsCollectionView { get; }

    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnInventoryFilterChanged(string value)
    {
        InventoryCollectionView.Refresh();
    }

    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnUnassignedItemsFilterChanged(string value)
    {
        UnassignedItemsCollectionView.Refresh();
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
        var filter = UnassignedItemsFilter;
        if (string.IsNullOrWhiteSpace(filter) || obj == null)
        {
            return true;
        }

        var item = (InventoryItem)obj;
        return FilterItem(item, filter);
    }

    private bool OnFilterInventory(object? obj)
    {
        var filter = InventoryFilter;
        if (string.IsNullOrWhiteSpace(filter) || obj == null)
        {
            return true;
        }

        var item = (InventoryItem)obj;
        return FilterItem(item, filter);
    }

    private static bool FilterItem(InventoryItem item, string filter)
    {
        return item.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
               item.NameDe.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
               item.Type.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
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
            m.SelectedSavegame.SavegameStore.LoadJson<PlayerInventoryDataModel>(SavegameStore.FileType
                .PlayerInventorySaveData);
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

    public bool Update(Savegame savegame, bool createBackup)
    {
        var playerInventoryData = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerInventorySaveData);

        if (playerInventoryData == null)
        {
            return false;
        }

        var selectedItems = _inventory
            .Select(item => item.ItemBlock)
            .ToList();

        if (!PlayerInventoryDataModel.Merge(playerInventoryData, selectedItems))
        {
            return false;
        }

        savegame.SavegameStore.StoreJson(SavegameStore.FileType.PlayerInventorySaveData, playerInventoryData,
            createBackup);

        return true;
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
    public string TypeRendered => Type.FirstCharToUpper();
}