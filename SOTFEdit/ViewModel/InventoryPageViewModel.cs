using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private readonly ObservableCollection<InventoryItem> _inventory = new();
    private readonly ItemList _itemList;
    private readonly ObservableCollection<InventoryItem> _unassignedItems = new();
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
        SetupListeners();
    }

    public GenericCollectionView<InventoryItem> InventoryCollectionView { get; }
    public GenericCollectionView<InventoryItem> UnassignedItemsCollectionView { get; }

    partial void OnInventoryFilterChanged(string value)
    {
        InventoryCollectionView.Refresh();
    }

    partial void OnUnassignedItemsFilterChanged(string value)
    {
        UnassignedItemsCollectionView.Refresh();
    }

    [RelayCommand]
    public void RemoveItemFromInventory(InventoryItem? inventoryItem)
    {
        if (inventoryItem == null)
        {
            return;
        }

        _unassignedItems.Add(inventoryItem);
        _inventory.Remove(inventoryItem);
    }

    [RelayCommand]
    public void AddUnassignedItem(InventoryItem? inventoryItem)
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
        if (string.IsNullOrWhiteSpace(UnassignedItemsFilter) || obj == null)
        {
            return true;
        }

        var item = (InventoryItem)obj;
        return FilterItem(item, filter.ToLower());
    }

    private bool OnFilterInventory(object? obj)
    {
        var filter = InventoryFilter;
        if (string.IsNullOrWhiteSpace(filter) || obj == null)
        {
            return true;
        }

        var item = (InventoryItem)obj;
        return FilterItem(item, filter.ToLower());
    }

    private static bool FilterItem(InventoryItem item, string filter)
    {
        return item.Name.ToLower().Contains(filter) || item.NameDe.ToLower().Contains(filter) ||
               item.Id.ToString().Contains(filter) || item.Type
                   .Contains(filter) || item.TotalCount.ToString().Contains(filter);
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent m)
    {
        _inventory.Clear();
        _unassignedItems.Clear();
        if (m.SelectedSavegame == null)
        {
            return;
        }

        HashSet<int> assignedItems = new();

        var saveData =
            m.SelectedSavegame.SavegameStore.LoadJson<PlayerInventoryDataModel>(SavegameStore.FileType
                .PlayerInventorySaveData);
        if (saveData != null)
        {
            foreach (var itemBlock in saveData.Data.PlayerInventory.ItemInstanceManagerData.ItemBlocks)
            {
                _inventory.Add(new InventoryItem(itemBlock, _itemList.GetItem(itemBlock.ItemId)));
                assignedItems.Add(itemBlock.ItemId);
            }
        }

        foreach (var item in _itemList)
        {
            if (!assignedItems.Contains(item.Key) && item.Value.IsInventoryItem)
            {
                _unassignedItems.Add(BuildUnassignedItem(item.Value));
            }
        }
    }

    private static InventoryItem BuildUnassignedItem(Item item)
    {
        var itemBlock = ItemBlockModel.Unassigned(item.Id);
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
}

public class InventoryItem
{
    private readonly Item? _item;

    public InventoryItem(ItemBlockModel itemBlock, Item? item)
    {
        ItemBlock = itemBlock;
        _item = item;
    }

    public ItemBlockModel ItemBlock { get; }

    public string Type => _item?.Type ?? "";

    public int Id => _item?.Id ?? ItemBlock.ItemId;

    public int TotalCount
    {
        get => ItemBlock.TotalCount;
        set => ItemBlock.TotalCount = value;
    }

    public string Name => _item?.Name ?? "??? Unknown Item";
    public string NameDe => _item?.NameDe ?? "";
}