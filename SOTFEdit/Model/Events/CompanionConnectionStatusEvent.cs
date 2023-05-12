using SOTFEdit.Infrastructure.Companion;

namespace SOTFEdit.Model.Events;

public class CompanionConnectionStatusEvent
{
    public CompanionConnectionStatusEvent(CompanionConnectionManager.ConnectionStatus status)
    {
        Status = status;
    }

    public CompanionConnectionManager.ConnectionStatus Status { get; }
}