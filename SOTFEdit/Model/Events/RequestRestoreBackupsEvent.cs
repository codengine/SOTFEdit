namespace SOTFEdit.Model.Events;

internal class RequestRestoreBackupsEvent(Savegame.Savegame savegame, bool restoreFromNewest)
{
    public Savegame.Savegame Savegame { get; } = savegame;
    public bool RestoreFromNewest { get; } = restoreFromNewest;
}