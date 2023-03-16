namespace SOTFEdit.Model.Events;

public class RequestReviveFollowersEvent
{
    public RequestReviveFollowersEvent(Savegame selectedSavegame, bool backupFiles, int typeId)
    {
        SelectedSavegame = selectedSavegame;
        BackupFiles = backupFiles;
        TypeId = typeId;
    }

    public Savegame SelectedSavegame { get; }
    public bool BackupFiles { get; }
    public int TypeId { get; }
}