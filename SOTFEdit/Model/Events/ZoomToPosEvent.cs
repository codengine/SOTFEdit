namespace SOTFEdit.Model.Events;

public class ZoomToPosEvent
{
    public ZoomToPosEvent(Position pos)
    {
        Pos = pos;
    }

    public Position Pos { get; }
}