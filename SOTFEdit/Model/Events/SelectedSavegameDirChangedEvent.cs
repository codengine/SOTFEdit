namespace SOTFEdit.Model.Events;

public class SelectedSavegameDirChangedEvent(string? newPath)
{
    public string? NewPath { get; } = newPath;
}