using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;

namespace SOTFEdit.ViewModel;

public partial class ActorViewModel
{
    public ActorViewModel(ActorCollection actorCollection)
    {
        ActorCollection = actorCollection;
    }

    public ActorCollection ActorCollection { get; }

    [RelayCommand]
    private static void OpenMap(Position? position)
    {
        if (position != null)
        {
            WeakReferenceMessenger.Default.Send(new ZoomToPosEvent(position));
        }
    }

    [RelayCommand]
    private static void EditActor(Actor actor)
    {
        WeakReferenceMessenger.Default.Send(new RequestEditActorEvent(actor));
    }
}