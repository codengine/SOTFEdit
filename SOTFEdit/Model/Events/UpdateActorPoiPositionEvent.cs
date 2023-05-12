namespace SOTFEdit.Model.Events;

public class UpdateActorPoiPositionEvent
{
    public UpdateActorPoiPositionEvent(int typeId, Position newPosition)
    {
        TypeId = typeId;
        NewPosition = newPosition;
    }

    public int TypeId { get; }
    public Position NewPosition { get; }
}