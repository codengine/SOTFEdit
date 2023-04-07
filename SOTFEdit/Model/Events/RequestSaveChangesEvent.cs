using System;

namespace SOTFEdit.Model.Events;

public class RequestSaveChangesEvent
{
    private readonly Action<ApplicationSettings.BackupMode> _saveChangesCallback;

    public RequestSaveChangesEvent(Savegame.Savegame selectedSavegame,
        Action<ApplicationSettings.BackupMode> saveChangesCallback)
    {
        SelectedSavegame = selectedSavegame;
        _saveChangesCallback = saveChangesCallback;
    }

    public Savegame.Savegame SelectedSavegame { get; }

    public void InvokeCallback(ApplicationSettings.BackupMode backupMode)
    {
        _saveChangesCallback.Invoke(backupMode);
    }
}