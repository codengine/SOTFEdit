using SOTFEdit.Infrastructure.Companion;

namespace SOTFEdit.Model.Events;

public class CompanionConnectionStatusEvent(CompanionConnectionManager.ConnectionStatus status)
{
    public CompanionConnectionManager.ConnectionStatus Status { get; } = status;
}