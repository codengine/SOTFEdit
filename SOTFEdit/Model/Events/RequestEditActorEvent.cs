using System.Windows;
using SOTFEdit.Model.Actors;

namespace SOTFEdit.Model.Events;

public class RequestEditActorEvent(Actor actor, Window? owner = null)
{
    public Actor Actor { get; } = actor;
    public Window? Owner { get; } = owner;
}