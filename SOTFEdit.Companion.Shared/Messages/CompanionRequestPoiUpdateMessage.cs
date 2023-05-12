using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public class CompanionRequestPoiUpdateMessage : ICompanionMessage
{
    [Key(0)]
    public PoiGroupType Type { get; set; }
}