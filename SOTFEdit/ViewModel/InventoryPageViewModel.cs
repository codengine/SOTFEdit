using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.SaveData;

namespace SOTFEdit.ViewModel;

public partial class InventoryPageViewModel : ObservableObject
{
    private readonly ObservableCollection<InventoryItem> _inventory = new();
    private readonly ItemList _itemList;
    private readonly ObservableCollection<InventoryItem> _unassignedItems = new();
    [ObservableProperty] private string _inventoryFilter = "";
    [ObservableProperty] private string _unassignedItemsFilter = "";

    public InventoryPageViewModel()
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
        _itemList = Ioc.Default.GetRequiredService<ItemList>();
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
    public void RemoveItemFromInventory(InventoryItem inventoryItem)
    {
        lock (this)
        {
            _unassignedItems.Add(inventoryItem);
            _inventory.Remove(inventoryItem);
        }
    }

    [RelayCommand]
    public void AddUnassignedItem(InventoryItem inventoryItem)
    {
        lock (this)
        {
            _inventory.Add(inventoryItem);
            _unassignedItems.Remove(inventoryItem);
        }
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
        WeakReferenceMessenger.Default.Register<SelectedSavegameChanged>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChanged m)
    {
        lock (this)
        {
            _inventory.Clear();
            _unassignedItems.Clear();
            if (m.SelectedSavegame == null)
            {
                return;
            }

            HashSet<int> assignedItems = new();

            var saveData =
                m.SelectedSavegame.SavegameStore.LoadJson<PlayerInventoryData>(SavegameStore.FileType
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
                if (!assignedItems.Contains(item.Key))
                {
                    _unassignedItems.Add(BuildUnassignedItem(item.Value));
                }
            }
        }
    }

    private static InventoryItem BuildUnassignedItem(Item item)
    {
        var itemBlock = ItemInstanceManagerData.ItemBlock.Unassigned(item.Id);
        return new InventoryItem(itemBlock, item);
    }

    public void Update(Savegame savegame, bool createBackup)
    {
        var playerInventoryData = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerInventorySaveData);

        if (playerInventoryData == null)
        {
            return;
        }

        lock (this)
        {
            var selectedItems = _inventory
                .Select(item => item.ItemBlock)
                .ToList();

            if (!PlayerInventoryData.Merge(playerInventoryData, selectedItems))
            {
                return;
            }

            savegame.SavegameStore.StoreJson(SavegameStore.FileType.PlayerInventorySaveData, playerInventoryData,
                createBackup);
        }
    }
}

public class InventoryItem
{
    private readonly Item? _item;

    public InventoryItem(ItemInstanceManagerData.ItemBlock itemBlock, Item? item)
    {
        ItemBlock = itemBlock;
        _item = item;
    }

    public ItemInstanceManagerData.ItemBlock ItemBlock { get; }

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