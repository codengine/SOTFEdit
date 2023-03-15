namespace SOTFEdit.Model.Events;

public class RequestRegrowTreesEvent
{
    public RequestRegrowTreesEvent(Savegame selectedSavegame, bool backupFiles, VegetationState vegetationStateSelected)
    {
        SelectedSavegame = selectedSavegame;
        BackupFiles = backupFiles;
        VegetationStateSelected = vegetationStateSelected;
    }

    public Savegame SelectedSavegame { get; }
    public bool BackupFiles { get; }
    public VegetationState VegetationStateSelected { get; }
}