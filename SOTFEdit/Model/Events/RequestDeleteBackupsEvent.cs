namespace SOTFEdit.Model.Events;

public class RequestDeleteBackupsEvent(Savegame.Savegame savegame)
{
    public Savegame.Savegame Savegame { get; } = savegame;
}