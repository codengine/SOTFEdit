namespace SOTFEdit.Model.Events;

public class RequestCheckForUpdatesEvent(bool notifyOnSameVersion, bool notifyOnError)
{
    public bool NotifyOnSameVersion { get; } = notifyOnSameVersion;
    public bool NotifyOnError { get; } = notifyOnError;
}