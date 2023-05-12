using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public class CompanionPoiListMessage : ICompanionMessage
{
    public CompanionPoiListMessage(PoiGroupType type, List<CompanionPoiMessage> pois)
    {
        Type = type;
        Pois = pois;
    }

    [Key(0)]
    public PoiGroupType Type { get; }

    [Key(1)]
    public List<CompanionPoiMessage> Pois { get; }
}