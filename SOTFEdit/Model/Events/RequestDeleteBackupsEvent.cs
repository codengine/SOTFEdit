namespace SOTFEdit.Model.Events;

public class RequestDeleteBackupsEvent
{
    public Savegame Savegame { get; }

    public RequestDeleteBackupsEvent(Savegame savegame)
    {
        Savegame = savegame;
    }
}