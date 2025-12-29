namespace SOTFEdit.Model.Events;

public class PlayerPosChangedEvent(Position newPosition)
{
    public Position NewPosition { get; } = newPosition;
}