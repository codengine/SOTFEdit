using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public record CompanionPosMessage : ICompanionMessage
{
    [Key(0)]
    public CharacterTarget Target { get; set; }

    [Key(1)]
    public float X { get; set; }

    [Key(2)]
    public float Y { get; set; }

    [Key(3)]
    public float Z { get; set; }

    [Key(4)]
    public float Rotation { get; set; }

    [Key(5)]
    public int Mask { get; set; }

    [Key(6)]
    public int InstanceId { get; set; }

    [Key(7)]
    public string? Name { get; set; }
}