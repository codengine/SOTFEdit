namespace SOTFEdit.Model.Events;

public class RequestCheckForUpdatesEvent
{
    public bool NotifyOnSameVersion { get; }
    public bool NotifyOnError { get; }

    public RequestCheckForUpdatesEvent(bool notifyOnSameVersion, bool notifyOnError)
    {
        NotifyOnSameVersion = notifyOnSameVersion;
        NotifyOnError = notifyOnError;
    }
}