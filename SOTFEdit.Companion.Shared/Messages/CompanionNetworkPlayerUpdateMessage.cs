using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public class CompanionNetworkPlayerUpdateMessage : ICompanionMessage
{
    [Key(0)]
    public HashSet<int>? Added { get; set; }

    [Key(1)]
    public HashSet<int>? Deleted { get; set; }
}