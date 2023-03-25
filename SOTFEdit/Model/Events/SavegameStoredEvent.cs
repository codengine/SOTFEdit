namespace SOTFEdit.Model.Events;

public class SavegameStoredEvent
{
    public SavegameStoredEvent(string? message, bool reloadSavegame = true)
    {
        Message = message;
        ReloadSavegame = reloadSavegame;
    }

    public string? Message { get; }
    public bool ReloadSavegame { get; }
}