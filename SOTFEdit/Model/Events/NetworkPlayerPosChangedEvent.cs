namespace SOTFEdit.Model.Events;

public class NetworkPlayerPosChangedEvent
{
    public NetworkPlayerPosChangedEvent(int instanceId, string? name, Position newPosition)
    {
        InstanceId = instanceId;
        Name = name;
        NewPosition = newPosition;
    }

    public int InstanceId { get; }
    public string? Name { get; }
    public Position NewPosition { get; }
}