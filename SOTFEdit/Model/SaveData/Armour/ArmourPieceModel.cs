namespace SOTFEdit.Model.SaveData.Armour;

public record ArmourPieceModel
{
    public int ItemId { get; init; }
    public int Slot { get; init; }
    public float RemainingArmourpoints { get; set; }
}