using System.Collections.Generic;

namespace SOTFEdit.Model.Events;

public class RequestReviveFollowersEvent
{
    public RequestReviveFollowersEvent(Savegame selectedSavegame, bool backupFiles, int typeId, HashSet<int> itemIds, Outfit? outfit, Position pos)
    {
        SelectedSavegame = selectedSavegame;
        BackupFiles = backupFiles;
        TypeId = typeId;
        ItemIds = itemIds;
        Outfit = outfit;
        Pos = pos;
    }

    public Savegame SelectedSavegame { get; }
    public bool BackupFiles { get; }
    public int TypeId { get; }
    public HashSet<int> ItemIds { get; }
    public Outfit? Outfit { get; }
    public Position Pos { get; }
}