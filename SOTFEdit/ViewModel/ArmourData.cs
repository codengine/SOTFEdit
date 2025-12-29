using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Armour;

namespace SOTFEdit.ViewModel;

public partial class ArmourData(ArmourPieceModel armourPieceModel, Item? item) : ObservableObject
{
    public ArmourPieceModel ArmourPiece { get; } = armourPieceModel;

    public int Id => item?.Id ?? ArmourPiece.ItemId;

    public string Name => item?.Name ?? TranslationManager.Get("armor.unknownItem");

    public int Slot => ArmourPiece.Slot;

    public int MinDurability => item?.Durability?.Min ?? 1;
    public int MaxDurability => item?.Durability?.Max ?? 65535;
    public int DefaultDurability => item?.Durability?.Default ?? 1;

    public BitmapImage? Image => item?.ThumbnailMedium;
    public BitmapImage? ImageBig => item?.ThumbnailBig;

    [RelayCommand]
    private void SetDurability(int? amount)
    {
        ArmourPiece.RemainingArmourpoints = amount ?? item?.Durability?.Min ?? 1;
    }
}