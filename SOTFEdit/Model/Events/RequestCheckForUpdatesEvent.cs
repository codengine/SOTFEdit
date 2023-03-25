namespace SOTFEdit.Model.Events;

public class RequestCheckForUpdatesEvent
{
    public RequestCheckForUpdatesEvent(bool notifyOnSameVersion, bool notifyOnError)
    {
        NotifyOnSameVersion = notifyOnSameVersion;
        NotifyOnError = notifyOnError;
    }

    public bool NotifyOnSameVersion { get; }
    public bool NotifyOnError { get; }
}