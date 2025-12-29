using SOTFEdit.Companion.Shared.Messages;

namespace SOTFEdit.Infrastructure.Companion;

public class CompanionAddPoiMessageHandler(CompanionPoiStorage poiStorage) : MessageHandler<CompanionAddPoiMessage>
{
    protected override void Handle(CompanionAddPoiMessage message)
    {
        poiStorage.Add(message);
    }
}