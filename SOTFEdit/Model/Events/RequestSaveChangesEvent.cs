using System;

namespace SOTFEdit.Model.Events;

public class RequestSaveChangesEvent(
    Savegame.Savegame selectedSavegame,
    Action<ApplicationSettings.BackupMode> saveChangesCallback)
{
    public Savegame.Savegame SelectedSavegame { get; } = selectedSavegame;

    public void InvokeCallback(ApplicationSettings.BackupMode backupMode)
    {
        saveChangesCallback.Invoke(backupMode);
    }
}