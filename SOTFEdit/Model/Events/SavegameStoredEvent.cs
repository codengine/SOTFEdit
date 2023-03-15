namespace SOTFEdit.Model.Events;

public class SavegameStoredEvent
{
    public SavegameStoredEvent(string message) : this(message, true)
    {
    }

    public SavegameStoredEvent(string message, bool reloadSavegame)
    {
        Message = message;
        ReloadSavegame = reloadSavegame;
    }

    public string Message { get; }
    public bool ReloadSavegame { get; }
}