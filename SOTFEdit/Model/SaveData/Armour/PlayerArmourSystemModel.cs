using System.Collections.Generic;
using System.Linq;

namespace SOTFEdit.Model.SaveData.Armour;

// ReSharper disable once ClassNeverInstantiated.Global
public record PlayerArmourSystemModel : SotfBaseModel
{
    private static readonly int[] ArmorSlots = { 6, 7, 10, 11, 4, 5, 8, 9, 1, 0 };
    public List<ArmourPieceModel> ArmourPieces { get; private set; } = new();

    public static bool Merge(PlayerArmourSystemModel armourSystemModel,
        List<ArmourPieceModel> selectedArmorPieces)
    {
        UpdateSlots(selectedArmorPieces);
        var hasChanges = !armourSystemModel.ArmourPieces.OrderBy(piece => piece.Slot)
            .SequenceEqual(selectedArmorPieces.OrderBy(piece => piece.Slot), ArmourPieceModel.ArmourPieceModelComparer);
        if (hasChanges)
        {
            armourSystemModel.ArmourPieces = new List<ArmourPieceModel>(selectedArmorPieces);
        }

        return hasChanges;
    }

    private static void UpdateSlots(IReadOnlyList<ArmourPieceModel> selectedArmorPieces)
    {
        for (var i = 0; i < selectedArmorPieces.Count; i++)
        {
            if (i >= ArmorSlots.Length)
            {
                break;
            }

            selectedArmorPieces[i].Slot = ArmorSlots[i];
        }
    }
}