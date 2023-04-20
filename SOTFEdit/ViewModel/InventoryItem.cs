using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Inventory;

namespace SOTFEdit.ViewModel;

public partial class InventoryItem : ObservableObject
{
    public Item? Item { get; }

    public InventoryItem(ItemBlockModel itemBlock, Item? item)
    {
        ItemBlock = itemBlock;
        Item = item;
    }

    public ItemBlockModel ItemBlock { get; }

    public string Type => Item?.Type ?? "";

    public string TypeRendered => string.IsNullOrEmpty(Item?.Type) ? "" : TranslationManager.Get("itemTypes." + Type);

    public int Id => Item?.Id ?? ItemBlock.ItemId;

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

    public string Name => Item?.Name ?? TranslationManager.Get("inventory.unknownItem");
    public string NormalizedName => Item?.NormalizedLowercaseName ?? TranslationManager.Get("inventory.unknownItem");

    public int Max => Item?.StorageMax?.Inventory ?? 1;

    [RelayCommand]
    private void SetToMax()
    {
        TotalCount = Max;
    }
}