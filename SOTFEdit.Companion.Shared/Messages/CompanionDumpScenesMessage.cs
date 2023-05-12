using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public record CompanionDumpScenesMessage : ICompanionMessage;