using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model.Actor;

public partial class VirginiaState : FollowerState
{
    [ObservableProperty] private float _affection;

    public VirginiaState(List<Outfit> outfits, IEnumerable<Item> equippableItems) : base(
        Constants.Actors.VirginiaTypeId, outfits, equippableItems)
    {
    }

    public override void Reset()
    {
        base.Reset();
        Affection = 0.0f;
    }
}