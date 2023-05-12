using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public class CompanionSettingsMessage : ICompanionMessage
{
    [Key(0)]
    public decimal PositionUpdateFrequency { get; set; }
}