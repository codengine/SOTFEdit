using System.Collections.Generic;

namespace SOTFEdit.Model.Events;

public class RequestReviveFollowersEvent(
    Savegame.Savegame selectedSavegame, int typeId,
    HashSet<int> itemIds, Outfit? outfit, Position pos)
{
    public Savegame.Savegame SelectedSavegame { get; } = selectedSavegame;
    public int TypeId { get; } = typeId;
    public HashSet<int> ItemIds { get; } = itemIds;
    public Outfit? Outfit { get; } = outfit;
    public Position Pos { get; } = pos;
}