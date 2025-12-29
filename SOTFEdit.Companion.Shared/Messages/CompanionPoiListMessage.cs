using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public class CompanionPoiListMessage(PoiGroupType type, List<CompanionPoiMessage> pois) : ICompanionMessage
{
    [Key(0)] public PoiGroupType Type { get; } = type;

    [Key(1)] public List<CompanionPoiMessage> Pois { get; } = pois;
}