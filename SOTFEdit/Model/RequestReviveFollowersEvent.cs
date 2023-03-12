namespace SOTFEdit.Model;

public class RequestReviveFollowersEvent
{
    public RequestReviveFollowersEvent(Savegame selectedSavegame, bool backupFiles)
    {
        SelectedSavegame = selectedSavegame;
        BackupFiles = backupFiles;
    }

    public Savegame SelectedSavegame { get; }
    public bool BackupFiles { get; }
}