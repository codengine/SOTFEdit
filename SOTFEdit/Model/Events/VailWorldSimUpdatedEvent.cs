using SOTFEdit.Model.Savegame;

namespace SOTFEdit.Model.Events;

public class JsonModelChangedEvent
{
    public JsonModelChangedEvent(SavegameStore.FileType fileType)
    {
        FileType = fileType;
    }

    public SavegameStore.FileType FileType { get; }
}