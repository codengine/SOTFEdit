namespace SOTFEdit.Model.Events;

public class SavegameStoredEvent
{
    public SavegameStoredEvent(string message)
    {
        Message = message;
    }

    public string Message { get; }
}