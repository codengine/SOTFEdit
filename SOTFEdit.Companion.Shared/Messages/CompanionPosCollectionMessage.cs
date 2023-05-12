using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public class CompanionPosCollectionMessage : ICompanionMessage
{
    [Key(0)]
    public List<CompanionPosMessage> Positions { get; set; }
}