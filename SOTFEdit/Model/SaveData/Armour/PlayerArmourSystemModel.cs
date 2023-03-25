using System.Collections.Generic;
using System.Linq;

namespace SOTFEdit.Model.SaveData.Armour;

// ReSharper disable once ClassNeverInstantiated.Global
public record PlayerArmourSystemModel : SotfBaseModel
{
    public List<ArmourPieceModel> ArmourPieces { get; private set; } = new();

    public static bool Merge(PlayerArmourSystemModel armourSystemModel,
        List<ArmourPieceModel> selectedArmorPieces)
    {
        var hasChanges = !armourSystemModel.ArmourPieces.OrderBy(piece => piece.Slot)
            .SequenceEqual(selectedArmorPieces.OrderBy(piece => piece.Slot));
        if (hasChanges)
        {
            armourSystemModel.ArmourPieces = new List<ArmourPieceModel>(selectedArmorPieces);
        }

        return hasChanges;
    }
}