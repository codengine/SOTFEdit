using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SOTFEdit.Model.SaveData;

public record PlayerArmourData : SotfBaseModel
{
    public DataModel Data { get; init; }

    public record DataModel
    {
        [JsonConverter(typeof(StringTypeConverter))]
        public PlayerArmourSystem PlayerArmourSystem { get; init; }
    }
}

public record PlayerArmourSystem : SotfBaseModel
{
    public List<ArmourPiece> ArmourPieces { get; set; } = new();

    public static bool Merge(PlayerArmourSystem armourSystem, List<ArmourPiece> selectedArmorPieces)
    {
        var hasChanges = !armourSystem.ArmourPieces.OrderBy(piece => piece.Slot)
            .SequenceEqual(selectedArmorPieces.OrderBy(piece => piece.Slot));
        if (hasChanges)
        {
            armourSystem.ArmourPieces = new List<ArmourPiece>(selectedArmorPieces);
        }

        return hasChanges;
    }
}

public record ArmourPiece
{
    public int ItemId { get; init; }
    public int Slot { get; init; }
    public float RemainingArmourpoints { get; set; }
}