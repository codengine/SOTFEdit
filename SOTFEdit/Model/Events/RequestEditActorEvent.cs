using System.Windows;
using SOTFEdit.Model.Actors;

namespace SOTFEdit.Model.Events;

public class RequestEditActorEvent
{
    public RequestEditActorEvent(Actor actor, Window? owner = null)
    {
        Actor = actor;
        Owner = owner;
    }

    public Actor Actor { get; }
    public Window? Owner { get; }
}