namespace SOTFEdit.Model.Events;

public class NetworkPlayerPosChangedEvent(int instanceId, string? name, Position newPosition)
{
    public int InstanceId { get; } = instanceId;
    public string? Name { get; } = name;
    public Position NewPosition { get; } = newPosition;
}