using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Inventory;

namespace SOTFEdit.ViewModel;

public partial class InventoryItem : ObservableObject
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
        set
        {
            if (ItemBlock.TotalCount == value)
            {
                return;
            }

            ItemBlock.TotalCount = value;
            OnPropertyChanged();
        }
    }

    public string Name => _item?.Name ?? "??? Unknown Item";
    public string NameDe => _item?.NameDe ?? "";

    public int Max => _item?.StorageMax?.Inventory ?? 1;

    [RelayCommand]
    private void SetToMax()
    {
        TotalCount = Max;
    }
}