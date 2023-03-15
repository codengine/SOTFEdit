namespace SOTFEdit.Model.Events;

public class SavegameStoredEvent
{
    public SavegameStoredEvent(string message) : this(message, true)
    {
    }

    public SavegameStoredEvent(string message, bool reloadSavegames)
    {
        Message = message;
        ReloadSavegames = reloadSavegames;
    }

    public string Message { get; }
    public bool ReloadSavegames { get; }
}