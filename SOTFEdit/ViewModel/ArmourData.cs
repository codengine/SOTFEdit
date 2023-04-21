using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Armour;

namespace SOTFEdit.ViewModel;

public partial class ArmourData : ObservableObject
{
    private readonly Item? _item;

    public ArmourData(ArmourPieceModel armourPieceModel, Item? item)
    {
        ArmourPiece = armourPieceModel;
        _item = item;
    }

    public ArmourPieceModel ArmourPiece { get; }

    public int Id => _item?.Id ?? ArmourPiece.ItemId;

    public string Name => _item?.Name ?? TranslationManager.Get("armor.unknownItem");

    public int Slot => ArmourPiece.Slot;

    public int MinDurability => _item?.Durability?.Min ?? 1;
    public int MaxDurability => _item?.Durability?.Max ?? 65535;
    public int DefaultDurability => _item?.Durability?.Default ?? 1;

    public BitmapImage? Image => _item?.ThumbnailMedium;
    public BitmapImage? ImageBig => _item?.ThumbnailBig;

    [RelayCommand]
    private void SetDurability(int? amount)
    {
        ArmourPiece.RemainingArmourpoints = amount ?? _item?.Durability?.Min ?? 1;
    }
}