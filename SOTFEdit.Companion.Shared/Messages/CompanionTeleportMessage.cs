using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public record CompanionTeleportMessage : ICompanionMessage
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
    public int AreaMask { get; set; }

    [Key(5)]
    public int GraphMask { get; set; }
}