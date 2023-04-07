using SOTFEdit.Model.Actors;

namespace SOTFEdit.Model.Events;

public class RequestEditActorEvent
{
    public RequestEditActorEvent(Actor actor)
    {
        Actor = actor;
    }

    public Actor Actor { get; }
}