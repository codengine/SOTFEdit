namespace SOTFEdit.Model.Events;

public class PlayerPosChangedEvent
{
    public PlayerPosChangedEvent(Position newPosition)
    {
        NewPosition = newPosition;
    }

    public Position NewPosition { get; }
}