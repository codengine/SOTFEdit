namespace SOTFEdit.Model.Events;

public class SavegameStoredEvent(string message)
{
    public string Message { get; } = message;
}