namespace SOTFEdit.Model.Events;

public class SelectedSavegameChangedEvent(Savegame.Savegame? selectedSavegame)
{
    public Savegame.Savegame? SelectedSavegame { get; } = selectedSavegame;
}