namespace SOTFEdit.Model.Events;

public class SelectedSavegameDirChangedEvent
{
    public SelectedSavegameDirChangedEvent(string? newPath)
    {
        NewPath = newPath;
    }

    public string? NewPath { get; }
}