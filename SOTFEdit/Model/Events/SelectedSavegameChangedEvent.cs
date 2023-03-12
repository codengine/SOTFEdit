namespace SOTFEdit.Model.Events;

public class SelectedSavegameChangedEvent
{
    public SelectedSavegameChangedEvent(Savegame? selectedSavegame)
    {
        SelectedSavegame = selectedSavegame;
    }

    public Savegame? SelectedSavegame { get; }
}