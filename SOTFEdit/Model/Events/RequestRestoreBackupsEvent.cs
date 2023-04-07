namespace SOTFEdit.Model.Events;

internal class RequestRestoreBackupsEvent
{
    public RequestRestoreBackupsEvent(Savegame.Savegame savegame, bool restoreFromNewest)
    {
        Savegame = savegame;
        RestoreFromNewest = restoreFromNewest;
    }

    public Savegame.Savegame Savegame { get; }
    public bool RestoreFromNewest { get; }
}