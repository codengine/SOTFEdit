using System.Collections.Generic;
using System.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Map;

public class MapManager
{
    private readonly ItemList _items;
    private readonly NpcsPageViewModel _npcsPageViewModel;
    private readonly StructuresPageViewModel _structuresPageViewModel;

    public MapManager(NpcsPageViewModel npcsPageViewModel, StructuresPageViewModel structuresPageViewModel,
        GameData gameData)
    {
        _npcsPageViewModel = npcsPageViewModel;
        _structuresPageViewModel = structuresPageViewModel;
        _items = gameData.Items;
    }

    public List<PoiGroup> GetActorPois()
    {
        return _npcsPageViewModel.AllActors.Select(actor => new ActorPoi(actor))
            .GroupBy(poi => poi.Actor.ActorType ?? new ActorType(-1, "???"))
            .OrderBy(kvp => kvp.Key.Name)
            .ToDictionary(group => group.Key, group => group.ToList())
            .Select(kvp => new PoiGroup(IsFollower(kvp.Value), new HashSet<IPoi>(kvp.Value), kvp.Key.Name,
                kvp.Key.IconPath.LoadAppLocalImage(24, 24)))
            .ToList();
    }

    private static bool IsFollower(List<ActorPoi> pois)
    {
        return pois.TrueForAll(poi =>
            poi.Actor.TypeId is Constants.Actors.KelvinTypeId or Constants.Actors.VirginiaTypeId);
    }

    public Dictionary<string, List<StructurePoi>> GetStructurePois()
    {
        return _structuresPageViewModel.Structures
            .Where(wrapper => wrapper.Position != null)
            .Select(structure => new StructurePoi(structure))
            .GroupBy(poi => poi.ScrewStructureWrapper.Name)
            .OrderBy(g => g.Key)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    public Dictionary<string, List<WorldItemPoi>> GetWorldItemPois(Savegame.Savegame savegame, ItemList itemList)
    {
        return WorldItemTeleporterViewModel.Load(_items, savegame, true)
            .Select(state => new WorldItemPoi(state, itemList.GetItem(state.ItemId)))
            .GroupBy(poi => poi.State.Group)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public static List<ZiplinePoi> GetZiplinePois(Savegame.Savegame savegame)
    {
        var result = new List<ZiplinePoi>();

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ZipLineManagerSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ZipLineManager)?["Ziplines"] is not { } ziplines)
        {
            return result;
        }

        result.AddRange(
            from token
                in ziplines
            let posA = token["_anchorAPosition"]?.ToObject<Position>()
            let posB = token["_anchorBPosition"]?.ToObject<Position>()
            where posA != null && posB != null
            select new ZiplinePoi(token, posA, posB)
        );

        return result;
    }
}