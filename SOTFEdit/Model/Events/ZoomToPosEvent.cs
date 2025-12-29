namespace SOTFEdit.Model.Events;

public class ZoomToPosEvent(Position pos)
{
    public Position Pos { get; } = pos;
}