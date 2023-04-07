namespace SOTFEdit.Model.Events;

public class SelectedSavegameChangedEvent
{
    public SelectedSavegameChangedEvent(Savegame.Savegame? selectedSavegame)
    {
        SelectedSavegame = selectedSavegame;
    }

    public Savegame.Savegame? SelectedSavegame { get; }
}