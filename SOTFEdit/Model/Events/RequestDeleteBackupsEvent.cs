namespace SOTFEdit.Model.Events;

public class RequestDeleteBackupsEvent
{
    public RequestDeleteBackupsEvent(Savegame savegame)
    {
        Savegame = savegame;
    }

    public Savegame Savegame { get; }
}