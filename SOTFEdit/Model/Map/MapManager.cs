using System.Collections.Generic;
using System.Linq;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Map;

public class MapManager
{
    private readonly List<ActorType> _actorTypes;
    private readonly ItemList _items;
    private readonly NpcsPageViewModel _npcsPageViewModel;
    private readonly StructuresPageViewModel _structuresPageViewModel;

    public MapManager(NpcsPageViewModel npcsPageViewModel, StructuresPageViewModel structuresPageViewModel,
        GameData gameData)
    {
        _npcsPageViewModel = npcsPageViewModel;
        _structuresPageViewModel = structuresPageViewModel;
        _items = gameData.Items;
        _actorTypes = gameData.ActorTypes;
    }

    public List<PoiGroup> GetActorPois()
    {
        var followerNonFollowerActors = _npcsPageViewModel.AllActors.Select(actor =>
            {
                var poi = new ActorPoi(actor);
                poi.SetEnabledNoRefresh(actor.ActorType?.IsFollower() ?? false);
                return poi;
            })
            .GroupBy(actorPoi => actorPoi.Actor.ActorType?.IsFollower() ?? false)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<PoiGroup>();

        if (followerNonFollowerActors.TryGetValue(true, out var followerPois))
        {
            var followerPoisSorted = followerPois
                .OrderBy(followerPoi => followerPoi.Actor.ActorType?.Name ?? "???")
                .ToList();

            var icon = _actorTypes.FirstOrDefault(type => type.Id == Constants.Actors.KelvinTypeId)?.IconPath
                .LoadAppLocalImage(24, 24);

            result.Add(new PoiGroup(true, followerPoisSorted, TranslationManager.Get("map.followers"),
                PoiGroupKeys.Actors + "_followers",
                PoiGroupType.Followers, icon));
        }

        if (followerNonFollowerActors.TryGetValue(false, out var actorPois))
        {
            var actorPoiGroups = actorPois.GroupBy(poi => poi.Actor.ActorType ?? new ActorType(-1, "???"))
                .OrderBy(kvp => kvp.Key.Name)
                .ToDictionary(group => group.Key, group => group.ToList())
                .Select(kvp => new PoiGroup(false, new HashSet<IPoi>(kvp.Value), kvp.Key.Name,
                    PoiGroupKeys.Actors + kvp.Key.Id,
                    PoiGroupType.Actors, kvp.Key.IconPath.LoadAppLocalImage(24, 24)))
                .ToList();
            result.AddRange(actorPoiGroups);
        }

        return result;
    }

    public Dictionary<string, List<StructurePoi>> GetStructurePois()
    {
        return _structuresPageViewModel.Structures
            .Where(wrapper => wrapper.Position != null && (wrapper.ScrewStructure?.ShowOnMap ?? false))
            .Select(structure => new StructurePoi(structure))
            .GroupBy(poi => poi.ScrewStructureWrapper.Name)
            .OrderBy(g => g.Key)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    public static Dictionary<string, List<WorldItemPoi>> GetWorldItemPois(Savegame.Savegame savegame)
    {
        return WorldItemTeleporterViewModel.Load(savegame, true)
            .Select(state => new WorldItemPoi(state))
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