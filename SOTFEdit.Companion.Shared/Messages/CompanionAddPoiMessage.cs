using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public class CompanionAddPoiMessage : ICompanionMessage
{
    [Key(0)]
    public string Title { get; set; } = string.Empty;

    [Key(1)]
    public string Description { get; set; } = string.Empty;

    [Key(2)]
    public byte[] Screenshot { get; set; } = Array.Empty<byte>();

    [Key(3)]
    public float X { get; set; }

    [Key(4)]
    public float Y { get; set; }

    [Key(5)]
    public float Z { get; set; }

    [Key(6)]
    public int AreaMask { get; set; }
}