using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Companion.Shared.Messages;
using SOTFEdit.Model.Map;
using SOTFEdit.Model.Map.Static;

namespace SOTFEdit.Infrastructure.Companion;

public class CompanionRequestPoiUpdateMessageHandler : MessageHandler<CompanionRequestPoiUpdateMessage>
{
    private readonly CompanionPoiStorage _companionPoiStorage;
    private readonly MapManager _mapManager;
    private readonly PoiLoader _poiLoader;

    public CompanionRequestPoiUpdateMessageHandler(PoiLoader poiLoader,
        MapManager mapManager, CompanionPoiStorage companionPoiStorage)
    {
        _poiLoader = poiLoader;
        _mapManager = mapManager;
        _companionPoiStorage = companionPoiStorage;
    }

    protected override void Handle(CompanionRequestPoiUpdateMessage message)
    {
        switch (message.Type)
        {
            case PoiGroupType.Bunkers:
            case PoiGroupType.Printers:
            case PoiGroupType.Laptops:
            case PoiGroupType.Caves:
            case PoiGroupType.Villages:
            case PoiGroupType.Helicopters:
            case PoiGroupType.Doors:
            case PoiGroupType.Lakes:
                SendPois(message.Type, _poiLoader.GetRawPois(message.Type));
                break;
            case PoiGroupType.Items:
                SendPois(PoiGroupType.Items, _poiLoader.GetItemPoisForCompanion());
                break;
            case PoiGroupType.WorldItems:
                SendWorldItemPois();
                break;
            case PoiGroupType.Custom:
                SendCustomPois();
                break;
            case PoiGroupType.Generic:
            case PoiGroupType.Actors:
            case PoiGroupType.Camps:
            case PoiGroupType.Info:
            case PoiGroupType.Crates:
            case PoiGroupType.Supply:
            case PoiGroupType.Ammo:
            case PoiGroupType.CannibalVillages:
            case PoiGroupType.Ponds:
            case PoiGroupType.Structures:
            case PoiGroupType.ZipLines:
            case PoiGroupType.Player:
            case PoiGroupType.Followers:
            default:
                break;
        }
    }

    private void SendCustomPois()
    {
        var pois = _companionPoiStorage.GetAll()
            .OrderBy(poi => poi.Title);
        SendPoiList(PoiGroupType.Custom, pois);
    }

    private static void SendWorldItemPois()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var worldItemPois = MapManager.GetWorldItemPois(selectedSavegame).Values
            .SelectMany(l => l)
            .OrderBy(poi => poi.Title);
        SendPoiList(PoiGroupType.WorldItems, worldItemPois);
    }

    private static void SendPois(PoiGroupType type, IPoiGrouper? pois)
    {
        switch (pois)
        {
            case null:
                return;
            case PoiGroup poiGroup:
                SendPoiGroup(type, poiGroup);
                break;
            case PoiGroupCollection poiGroupCollection:
                SendPoiGroupCollection(type, poiGroupCollection);
                break;
        }
    }

    private static void SendPoiGroup(PoiGroupType type, PoiGroup poiGroup)
    {
        var poiList = poiGroup.Pois.Where(FilterPoi)
            .OrderBy(poi => poi.Title)
            .ToList();

        SendPoiList(type, poiList);
    }

    private static void SendPoiGroupCollection(PoiGroupType type, PoiGroupCollection poiGroupCollection)
    {
        var poiList = poiGroupCollection.PoiGroups
            .OrderBy(g => g.Title)
            .SelectMany(col => col.Pois)
            .Where(FilterPoi)
            .OrderBy(poi => poi.Title)
            .ToList();
        SendPoiList(type, poiList);
    }

    private static bool FilterPoi(IPoi poi)
    {
        return poi.Position != null;
    }

    private static void SendPoiList(PoiGroupType type, IEnumerable<IPoi> poiList)
    {
        Ioc.Default.GetRequiredService<CompanionConnectionManager>()
            .SendAsync(new CompanionPoiListMessage(type, poiList.Select(ToCompanionPoiMessage).ToList()));
    }

    private static void SendPoiList(PoiGroupType type, IEnumerable<CustomPoi> poiList)
    {
        Ioc.Default.GetRequiredService<CompanionConnectionManager>()
            .SendAsync(new CompanionPoiListMessage(type, poiList.Select(ToCompanionPoiMessage).ToList()));
    }

    private static CompanionPoiMessage ToCompanionPoiMessage(IPoi poi)
    {
        var position = poi.Position!;

        string? screenshot = null;

        if (poi is InformationalPoi informationalPoi)
        {
            screenshot = informationalPoi.ScreenshotSmall;
        }

        return new CompanionPoiMessage(poi.Title, poi.Description, position.X, position.Y, position.Z,
            position.Area.AreaMask, screenshot);
    }

    private static CompanionPoiMessage ToCompanionPoiMessage(CustomPoi poi)
    {
        return new CompanionPoiMessage(poi.Title, poi.Description, poi.X, poi.Y, poi.Z, poi.AreaMask,
            CompanionPoiStorage.GetFullFilePath(poi.ScreenshotFile?.ExtendFilenameWith("_tn")));
    }
}