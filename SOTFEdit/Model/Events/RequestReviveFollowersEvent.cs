using System.Collections.Generic;

namespace SOTFEdit.Model.Events;

public class RequestReviveFollowersEvent
{
    public RequestReviveFollowersEvent(Savegame.Savegame selectedSavegame, int typeId,
        HashSet<int> itemIds, Outfit? outfit, Position pos)
    {
        SelectedSavegame = selectedSavegame;

        TypeId = typeId;
        ItemIds = itemIds;
        Outfit = outfit;
        Pos = pos;
    }

    public Savegame.Savegame SelectedSavegame { get; }
    public int TypeId { get; }
    public HashSet<int> ItemIds { get; }
    public Outfit? Outfit { get; }
    public Position Pos { get; }
}