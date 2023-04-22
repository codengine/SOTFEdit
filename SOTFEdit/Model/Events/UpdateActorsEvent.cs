using System.Collections.Generic;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.SaveData.Actor;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Events;

public class UpdateActorsEvent
{
    public UpdateActorsEvent(EditActorViewModel viewModel)
    {
        Influences = new List<Influence>(viewModel.Influences);
        ActorSelection = viewModel.ActorSelection;
        ModificationMode = viewModel.ModificationMode;
        ModifyOptions = viewModel.ModifyOptions;
        SkipKelvin = viewModel.SkipKelvin;
        SkipVirginia = viewModel.SkipVirginia;
        Actor = viewModel.Actor;
        OnlyInSameAreaAsActor = viewModel.OnlyInSameAreaAsActor;
    }

    public List<Influence> Influences { get; }

    public bool OnlyInSameAreaAsActor { get; }

    public Actor Actor { get; }

    public bool SkipVirginia { get; }

    public bool SkipKelvin { get; }

    public ModifyOptions ModifyOptions { get; }

    public ActorModificationMode ModificationMode { get; }

    public short? ActorSelection { get; }
}