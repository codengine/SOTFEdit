using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Inventory;

namespace SOTFEdit.ViewModel;

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