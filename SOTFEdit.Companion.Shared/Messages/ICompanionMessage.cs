using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[Union(0, typeof(CompanionPosMessage))]
[Union(1, typeof(CompanionPosCollectionMessage))]
[Union(2, typeof(CompanionTeleportMessage))]
[Union(3, typeof(CompanionAddPoiMessage))]
[Union(4, typeof(CompanionPoiListMessage))]
[Union(5, typeof(CompanionPoiMessage))]
[Union(6, typeof(CompanionDumpScenesMessage))]
[Union(7, typeof(CompanionRequestPoiUpdateMessage))]
[Union(8, typeof(CompanionNetworkPlayerUpdateMessage))]
[Union(9, typeof(CompanionSettingsMessage))]
public interface ICompanionMessage
{
}