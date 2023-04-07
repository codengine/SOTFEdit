namespace SOTFEdit.Model.Events;

public class RequestDeleteBackupsEvent
{
    public RequestDeleteBackupsEvent(Savegame.Savegame savegame)
    {
        Savegame = savegame;
    }

    public Savegame.Savegame Savegame { get; }
}