using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model.Actor;

public partial class KelvinState : FollowerState
{
    [ObservableProperty] private float _fear;

    public KelvinState(List<Outfit> outfits, IEnumerable<Item> equippableItems) : base(Constants.Actors.KelvinTypeId,
        outfits, equippableItems)
    {
    }

    public override void Reset()
    {
        base.Reset();
        Fear = 0.0f;
    }
}