namespace SOTFEdit.Model.Events;

public class UpdateActorPoiPositionEvent(int typeId, Position newPosition)
{
    public int TypeId { get; } = typeId;
    public Position NewPosition { get; } = newPosition;
}