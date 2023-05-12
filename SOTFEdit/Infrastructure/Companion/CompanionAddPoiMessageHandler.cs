using SOTFEdit.Companion.Shared.Messages;

namespace SOTFEdit.Infrastructure.Companion;

public class CompanionAddPoiMessageHandler : MessageHandler<CompanionAddPoiMessage>
{
    private readonly CompanionPoiStorage _poiStorage;

    public CompanionAddPoiMessageHandler(CompanionPoiStorage poiStorage)
    {
        _poiStorage = poiStorage;
    }

    protected override void Handle(CompanionAddPoiMessage message)
    {
        _poiStorage.Add(message);
    }
}