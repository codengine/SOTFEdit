using System;

namespace SOTFEdit.Model.Events;

public class RequestSaveChangesEvent
{
    private readonly bool _createBackup;
    private readonly Action<bool> _saveChangesCallback;

    public RequestSaveChangesEvent(Savegame selectedSavegame, bool createBackup, Action<bool> saveChangesCallback)
    {
        _createBackup = createBackup;
        SelectedSavegame = selectedSavegame;
        _saveChangesCallback = saveChangesCallback;
    }

    public Savegame SelectedSavegame { get; }

    public void InvokeCallback()
    {
        _saveChangesCallback.Invoke(_createBackup);
    }
}