using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Armour;

namespace SOTFEdit.ViewModel;

public record ArmourData
{
    private readonly Item? _item;

    public ArmourData(ArmourPieceModel armourPieceModel, Item? item)
    {
        ArmourPiece = armourPieceModel;
        _item = item;
    }

    public ArmourPieceModel ArmourPiece { get; }

    public int Id => _item?.Id ?? ArmourPiece.ItemId;

    // ReSharper disable once UnusedMember.Global
    public float RemainingArmourpoints
    {
        get => ArmourPiece.RemainingArmourpoints;
        set => ArmourPiece.RemainingArmourpoints = value;
    }

    public string Name => _item?.Name ?? "??? Unknown Item";
    public string NameDe => _item?.NameDe ?? "";

    public int Slot => ArmourPiece.Slot;
}