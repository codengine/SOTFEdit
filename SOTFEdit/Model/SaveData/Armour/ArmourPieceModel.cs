using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model.SaveData.Armour;

public partial class ArmourPieceModel : ObservableObject
{
    [ObservableProperty]
    private float _remainingArmourpoints;

    public int ItemId { get; init; }
    public int Slot { get; set; }

    public static IEqualityComparer<ArmourPieceModel> ArmourPieceModelComparer { get; } =
        new ArmourPieceModelEqualityComparer();

    private sealed class ArmourPieceModelEqualityComparer : IEqualityComparer<ArmourPieceModel>
    {
        public bool Equals(ArmourPieceModel? x, ArmourPieceModel? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x._remainingArmourpoints.Equals(y._remainingArmourpoints) && x.ItemId == y.ItemId &&
                   x.Slot == y.Slot;
        }

        public int GetHashCode(ArmourPieceModel obj)
        {
            return HashCode.Combine(obj._remainingArmourpoints, obj.ItemId, obj.Slot);
        }
    }
}