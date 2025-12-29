using SOTFEdit.Model.Savegame;

namespace SOTFEdit.Model.Events;

public class JsonModelChangedEvent(SavegameStore.FileType fileType)
{
    public SavegameStore.FileType FileType { get; } = fileType;
}