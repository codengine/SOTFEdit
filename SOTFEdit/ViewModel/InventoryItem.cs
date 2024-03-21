using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Inventory;

namespace SOTFEdit.ViewModel;

public partial class InventoryItem : ObservableObject
{
    public InventoryItem(ItemBlockModel itemBlock, Item? item, bool isEquipped = false)
    {
        ItemBlock = itemBlock;
        Item = item;
        IsEquipped = isEquipped;
    }

    public Item? Item { get; }
    public bool IsEquipped { get; }

    public ItemBlockModel ItemBlock { get; }

    public string Type => Item?.Type ?? "";

    public string TypeRendered => string.IsNullOrEmpty(Item?.Type) ? "" : TranslationManager.Get("itemTypes." + Type);

    public int Id => Item?.Id ?? ItemBlock.ItemId;

    public BitmapImage? Image => Item?.ThumbnailMedium;
    public BitmapImage? ImageBig => Item?.ThumbnailBig;

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