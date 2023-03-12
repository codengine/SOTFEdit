namespace SOTFEdit.Model;

public class RequestRegrowTreesEvent
{
    public RequestRegrowTreesEvent(Savegame selectedSavegame, bool backupFiles)
    {
        SelectedSavegame = selectedSavegame;
        BackupFiles = backupFiles;
    }

    public Savegame SelectedSavegame { get; }
    public bool BackupFiles { get; }
}