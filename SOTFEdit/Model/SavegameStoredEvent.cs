namespace SOTFEdit.Model;

public class SavegameStoredEvent
{
    public SavegameStoredEvent(string message)
    {
        Message = message;
    }

    public string Message { get; }
}