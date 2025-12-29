using System.Collections.Generic;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.SaveData.Actor;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Events;

public class UpdateActorsEvent(EditActorViewModel viewModel)
{
    public List<Influence> Influences { get; } = [..viewModel.Influences];

    public bool OnlyInSameAreaAsActor { get; } = viewModel.OnlyInSameAreaAsActor;

    public Actor Actor { get; } = viewModel.Actor;

    public bool SkipVirginia { get; } = viewModel.SkipVirginia;

    public bool SkipKelvin { get; } = viewModel.SkipKelvin;

    public ModifyOptions ModifyOptions { get; } = viewModel.ModifyOptions;

    public ActorModificationMode ModificationMode { get; } = viewModel.ModificationMode;

    public short? ActorSelection { get; } = viewModel.ActorSelection;
}