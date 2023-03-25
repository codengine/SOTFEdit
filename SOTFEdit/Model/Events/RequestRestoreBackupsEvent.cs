namespace SOTFEdit.Model.Events;

internal class RequestRestoreBackupsEvent
{
    public RequestRestoreBackupsEvent(Savegame savegame, bool restoreFromNewest)
    {
        Savegame = savegame;
        RestoreFromNewest = restoreFromNewest;
    }

    public Savegame Savegame { get; }
    public bool RestoreFromNewest { get; }
}