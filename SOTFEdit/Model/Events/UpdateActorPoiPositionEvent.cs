namespace SOTFEdit.Model.Events;

public class UpdateActorPoiPositionEvent
{
    public UpdateActorPoiPositionEvent(int uniqueId, Position newPosition)
    {
        UniqueId = uniqueId;
        NewPosition = newPosition;
    }

    public int UniqueId { get; }
    public Position NewPosition { get; }
}