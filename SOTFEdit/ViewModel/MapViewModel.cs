using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Companion.Shared.Messages;
using SOTFEdit.Infrastructure;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;

namespace SOTFEdit.ViewModel;

public partial class MapViewModel : ObservableObject
{
    private const char SaveSelectedPoiGroupSeparator = '|';

    private static readonly SolidColorBrush DarkMapColorBrush;
    private static readonly SolidColorBrush BrightMapColorBrush;

    private static readonly Brush DarkMapNetworkPlayerForeground = Brushes.White;
    private static readonly Brush BrightMapNetworkPlayerForeground = Brushes.Black;

    private readonly DispatcherTimer _fullTextFilterDispatcherTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(1000)
    };

    private readonly GameData _gameData;
    private readonly MapManager _mapManager;

    private readonly ObservableCollectionEx<IPoiGrouper> _poiGroups;

    private readonly HashSet<IPoi> _poisAdded = new();

    [ObservableProperty]
    private bool _followPlayer = true;

    [ObservableProperty]
    private bool _isNotConnected = true;

    [NotifyPropertyChangedFor(nameof(MapImageSource))]
    [NotifyPropertyChangedFor(nameof(MapBackground))]
    [NotifyPropertyChangedFor(nameof(NetworkPlayerForeground))]
    [ObservableProperty]
    private MapType _mapSelection = Settings.Default.MapType;

    [ObservableProperty]
    private IPoi? _selectedPoi;

    static MapViewModel()
    {
        var converter = new BrushConverter();
        DarkMapColorBrush = (SolidColorBrush)converter.ConvertFromString("#6186be")!;
        BrightMapColorBrush = (SolidColorBrush)converter.ConvertFromString("#9db4d4")!;
    }

    public MapViewModel(IEnumerable<IPoiGrouper> poiGroupers, MapManager mapManager, GameData gameData)
    {
        _mapManager = mapManager;
        _gameData = gameData;

        var poiGrouperList = poiGroupers.ToList();
        AddFollowersIfMissing(poiGrouperList);
        LoadSavedSelectedPois(poiGrouperList);

        _poiGroups = new ObservableCollectionEx<IPoiGrouper>(poiGrouperList);

        PoiGroups = CollectionViewSource.GetDefaultView(_poiGroups);
        PoiGroups.SortDescriptions.Add(new SortDescription("BaseTitle", ListSortDirection.Ascending));

        var enabled = _poiGroups.SelectMany(grouper =>
        {
            return grouper switch
            {
                PoiGroup poiGroup => poiGroup.Pois,
                PoiGroupCollection poiGroupCollection => poiGroupCollection.PoiGroups.SelectMany(g => g.Pois),
                _ => Enumerable.Empty<BasePoi>()
            };
        }).Where(poi => poi.Enabled).ToList();

        Pois = new ObservableCollectionEx<IPoi>(enabled.Where(poi => _poisAdded.Add(poi)));

        MapFilter = new MapFilter(gameData.AreaManager);
        MapFilter.PropertyChanged += MapFilterOnPropertyChanged;

        _fullTextFilterDispatcherTimer.Tick += OnFullTextFilter;

        SetupListeners();
    }

    public Brush NetworkPlayerForeground => GetNetworkPlayerBrushForMapMode();

    public string MapImageSource => GetMapSource(MapSelection);
    public Brush MapBackground => GetMapBackground(MapSelection);

    public MapFilter MapFilter { get; }

    public ObservableCollectionEx<IPoi> Pois { get; }

    public ICollectionView PoiGroups { get; }

    private Brush GetNetworkPlayerBrushForMapMode()
    {
        switch (MapSelection)
        {
            case MapType.Dark:
                return DarkMapNetworkPlayerForeground;
            case MapType.Original:
            default:
                return BrightMapNetworkPlayerForeground;
        }
    }

    private static Brush GetMapBackground(MapType mapSelection)
    {
        return mapSelection switch
        {
            MapType.Dark => DarkMapColorBrush,
            MapType.Original => BrightMapColorBrush,
            _ => DarkMapColorBrush
        };
    }

    private static string GetMapSource(MapType mapType)
    {
        return mapType switch
        {
            MapType.Dark => "pack://application:,,,/SOTFEdit;component/images/map/dark.jpg",
            MapType.Original => "pack://application:,,,/SOTFEdit;component/images/map/bright.jpg",
            _ => ""
        };
    }

    private void AddFollowersIfMissing(ICollection<IPoiGrouper> poiList)
    {
        var actorGroupCollection = poiList.OfType<PoiGroupCollection>()
            .FirstOrDefault(grouper => grouper.GroupType == PoiGroupType.Actors);

        if (actorGroupCollection == null)
        {
            actorGroupCollection = new PoiGroupCollection(true, TranslationManager.Get("map.actors"),
                PoiGroupKeys.Actors,
                new List<PoiGroup>(), PoiGroupType.Actors);
            poiList.Add(actorGroupCollection);
        }

        var followerGroup =
            actorGroupCollection.PoiGroups.FirstOrDefault(group => group.GroupType == PoiGroupType.Followers);
        if (followerGroup == null)
        {
            var icon = _gameData.ActorTypes.FirstOrDefault(type => type.Id == Constants.Actors.KelvinTypeId)?.IconPath
                .LoadAppLocalImage(24, 24);

            followerGroup = new PoiGroup(true, new List<IPoi>(), TranslationManager.Get("map.followers"),
                PoiGroupType.Followers, icon);
            actorGroupCollection.PoiGroups.Insert(0, followerGroup);
        }

        var kelvinInGroup = false;
        var virginiaInGroup = false;

        foreach (var poi in followerGroup.Pois.OfType<ActorPoi>())
        {
            switch (poi.Actor.TypeId)
            {
                case Constants.Actors.KelvinTypeId:
                    kelvinInGroup = true;
                    break;
                case Constants.Actors.VirginiaTypeId:
                    virginiaInGroup = true;
                    break;
            }
        }

        if (!kelvinInGroup)
        {
            var actorType = _gameData.ActorTypes.FirstOrDefault(type => type.Id == Constants.Actors.KelvinTypeId);
            followerGroup.Pois.Add(new ActorPoi(new Actor
            {
                ActorType = actorType,
                TypeId = Constants.Actors.KelvinTypeId,
                Position = new Position(0, 0, 0)
            }));
        }

        if (!virginiaInGroup)
        {
            var actorType = _gameData.ActorTypes.FirstOrDefault(type => type.Id == Constants.Actors.VirginiaTypeId);
            followerGroup.Pois.Add(new ActorPoi(new Actor
            {
                ActorType = actorType,
                TypeId = Constants.Actors.VirginiaTypeId,
                Position = new Position(0, 0, 0)
            }));
        }
    }

    private void OnFullTextFilter(object? sender, EventArgs e)
    {
        _fullTextFilterDispatcherTimer.Stop();
        ApplyFilterToAllPois();
    }

    partial void OnSelectedPoiChanging(IPoi? value)
    {
        if (_selectedPoi is { } selectedPoi)
        {
            selectedPoi.IsSelected = false;

            if (selectedPoi is IClickToMovePoi { IsMoveRequested: true } clickToMovePoi &&
                value is { Position: { } position })
            {
                clickToMovePoi.AcceptNewPos(position);
            }
        }

        if (value is BasePoi basePoi)
        {
            basePoi.IsSelected = true;
        }
    }

    partial void OnSelectedPoiChanged(IPoi? value)
    {
        PoiMessenger.Instance.Send(new SelectedPoiChangedEvent(value != null));
    }

    private void MapFilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapFilter.FullText) && !string.IsNullOrWhiteSpace(MapFilter.FullText))
        {
            _fullTextFilterDispatcherTimer.Start();
        }
        else
        {
            _fullTextFilterDispatcherTimer.Stop();
            ApplyFilterToAllPois();
        }
    }

    private void ApplyFilterToAllPois()
    {
        lock (Pois)
        {
            foreach (var poi in Pois)
            {
                poi.ApplyFilter(MapFilter);
            }
        }
    }

    private void SetupListeners()
    {
        PoiMessenger.Instance.Register<PoiRefreshEvent>(this,
            (_, message) => RefreshPois(message));
        PoiMessenger.Instance.Register<NpcsReloadedEvent>(this,
            (_, _) => OnNpcsReloaded());
        PoiMessenger.Instance.Register<RemovePoiEvent>(this,
            (_, message) => OnRemovePoiEvent(message));
        PoiMessenger.Instance.Register<ReapplyPoiFilterEvent>(this, (_, message) => OnReapplyPoiFilterEvent(message));
        PoiMessenger.Instance.Register<WorldItemsChangedEvent>(this, (_, _) => OnWorldItemsChangedEvent());
        PoiMessenger.Instance.Register<DeleteCustomPoiEvent>(this, (_, message) => OnDeleteCustomPoiEvent(message.Id));
        PoiMessenger.Instance.Register<CustomPoiAddedEvent>(this, (_, message) => OnCustomPoiAddedEvent(message.Poi));
        PoiMessenger.Instance.Register<AddZipPoiEvent>(this, (_, message) => OnAddZipPoiEvent(message.Poi));
        PoiMessenger.Instance.Register<CompanionNetworkPlayerUpdateMessage>(this,
            (_, message) =>
                Application.Current.Dispatcher.Invoke(() => OnCompanionNetworkPlayerUpdateMessage(message)));
        WeakReferenceMessenger.Default.Register<CompanionConnectionStatusEvent>(this,
            (_, message) => OnCompanionConnectionStatusEvent(message.Status));
    }

    private void OnCompanionConnectionStatusEvent(CompanionConnectionManager.ConnectionStatus status)
    {
        IsNotConnected = status != CompanionConnectionManager.ConnectionStatus.Connected;
    }

    private void OnAddZipPoiEvent(ZiplinePoi poi)
    {
        var ziplineGroup = _poiGroups.OfType<PoiGroup>()
            .FirstOrDefault(group => group.GroupType == PoiGroupType.ZipLines);
        if (ziplineGroup == null)
        {
            return;
        }

        var newPois = new List<IPoi>
        {
            poi.PointA,
            poi.PointB,
            poi
        };

        foreach (var newPoi in newPois)
        {
            newPoi.SetEnabledNoRefresh(ziplineGroup.Enabled);
            newPoi.ApplyFilter(MapFilter);
            ziplineGroup.Pois.Add(newPoi);
        }

        AddPois(newPois);
        PoiGroups.Refresh();
        SelectedPoi = poi.PointB;
    }

    private void OnCompanionNetworkPlayerUpdateMessage(CompanionNetworkPlayerUpdateMessage message)
    {
        var playerGroup = _poiGroups.OfType<PoiGroup>().FirstOrDefault(group => group.GroupType == PoiGroupType.Player);
        if (playerGroup == null)
        {
            return;
        }

        var existingNetworkPois = playerGroup.Pois.OfType<NetworkPlayerPoi>()
            .ToDictionary(poi => poi.InstanceId);

        var addedPois = new List<IPoi>();
        var deletedPois = new List<IPoi>();

        foreach (var instanceId in message.Added ?? Enumerable.Empty<int>())
        {
            if (existingNetworkPois.ContainsKey(instanceId))
            {
                continue;
            }

            var networkPlayerPoi = new NetworkPlayerPoi(new Position(0, 0, 0), instanceId, "???");
            networkPlayerPoi.SetEnabledNoRefresh(playerGroup.Enabled);
            networkPlayerPoi.ApplyFilter(MapFilter);
            playerGroup.Pois.Add(networkPlayerPoi);
            addedPois.Add(networkPlayerPoi);
        }

        if (addedPois.Count > 0)
        {
            AddPois(addedPois);
        }

        foreach (var instanceId in message.Deleted ?? Enumerable.Empty<int>())
        {
            if (existingNetworkPois.TryGetValue(instanceId, out var poi))
            {
                deletedPois.Add(poi);
            }
        }

        if (deletedPois.Count > 0)
        {
            OnRemovePoiEvent(new RemovePoiEvent(deletedPois));
        }

        PoiGroups.Refresh();
    }

    private void OnCustomPoiAddedEvent(CustomPoi poi)
    {
        var customMapPoi = CustomMapPoi.FromCustomPoi(poi, _gameData.AreaManager);
        var poiGrouper = GetPoiGrouperForCustom();
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (poiGrouper is PoiGroup poiGroup)
            {
                poiGroup.Pois.Add(customMapPoi);
            }

            Pois.Add(customMapPoi);
            PoiGroups.Refresh();
        });
    }

    private void OnDeleteCustomPoiEvent(int id)
    {
        var poisToDelete = Pois.Where(poi => poi is CustomMapPoi customMapPoi && customMapPoi.Id == id)
            .ToHashSet();

        if (poisToDelete.Count == 0)
        {
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            if (SelectedPoi != null && poisToDelete.Contains(SelectedPoi))
            {
                SelectedPoi = null;
            }

            foreach (var poi in poisToDelete)
            {
                _poisAdded.Remove(poi);
                Pois.Remove(poi);
            }

            if (GetPoiGrouperForCustom() is PoiGroup poiGroup)
            {
                var countRemoved = poiGroup.RemoveWhere(poi => poisToDelete.Contains(poi));
                if (countRemoved > 0)
                {
                    PoiGroups.Refresh();
                }
            }
        });
    }

    private IPoiGrouper? GetPoiGrouperForCustom()
    {
        var poiGrouper = _poiGroups.FirstOrDefault(group => group.GroupType == PoiGroupType.Custom);
        return poiGrouper;
    }

    private void OnWorldItemsChangedEvent()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var newWorldItemGroups = MapManager.GetWorldItemPois(selectedSavegame)
            .Select(kvp =>
                new PoiGroup(false, kvp.Value, kvp.Key, PoiGroupKeys.WorldItems + kvp.Key, PoiGroupType.WorldItems,
                    kvp.Value.First().IconSmall))
            .ToList();

        var oldWorldItemGroups = _poiGroups.Where(group => group.GroupType == PoiGroupType.WorldItems).ToList();

        if (oldWorldItemGroups.Count == 0)
        {
            foreach (var newWorldItemGroup in newWorldItemGroups)
            {
                _poiGroups.Add(newWorldItemGroup);
            }

            return;
        }

        foreach (var group in oldWorldItemGroups)
        {
            _poiGroups.Remove(group);
        }

        _poisAdded.RemoveWhere(poi => poi is WorldItemPoi);

        var oldPoiGroupsByTitle = oldWorldItemGroups
            .ToDictionary(group => group.BaseTitle);

        foreach (var poiGroup in newWorldItemGroups)
        {
            if (oldPoiGroupsByTitle.GetValueOrDefault(poiGroup.BaseTitle) is { } existingOldGroupByTitle)
            {
                poiGroup.SetEnabledNoRefresh(existingOldGroupByTitle.Enabled);
            }

            foreach (var poi in poiGroup.Pois)
            {
                poi.ApplyFilter(MapFilter);
            }
        }

        if (SelectedPoi is WorldItemPoi)
        {
            SelectedPoi = null;
        }

        var newPois = newWorldItemGroups.SelectMany(group => group.Pois).ToList();
        var addedPois = newPois.Where(poi => poi.Enabled && _poisAdded.Add(poi)).ToList();

        if (addedPois.Count > 0)
        {
            Pois.RemoveAndAdd(poi => poi is WorldItemPoi, addedPois);
        }
    }

    private void OnReapplyPoiFilterEvent(ReapplyPoiFilterEvent message)
    {
        message.Poi.ApplyFilter(MapFilter);
    }

    private void OnRemovePoiEvent(RemovePoiEvent message)
    {
        if (SelectedPoi is { } selectedPoi && message.Pois.Contains(selectedPoi))
        {
            SelectedPoi = null;
        }

        var removed = message.Pois.Where(poi => _poisAdded.Remove(poi)).ToList();

        if (removed.Count > 0)
        {
            foreach (var poi in removed)
            {
                Pois.Remove(poi);
            }
        }

        foreach (var grouper in _poiGroups)
        {
            switch (grouper)
            {
                case PoiGroup poiGroup:
                    poiGroup.Remove(message.Pois);
                    break;
                case PoiGroupCollection poiGroupCollection:
                    poiGroupCollection.Remove(message.Pois);
                    break;
            }
        }

        PoiGroups.Refresh();
    }

    private void OnNpcsReloaded()
    {
        var oldActorGroup = _poiGroups.OfType<PoiGroupCollection>()
            .FirstOrDefault(group => group.GroupType == PoiGroupType.Actors);
        var newActorPoiGroups = _mapManager.GetActorPois();

        var newActorGroupCollection = PoiGroupCollection.ForActors(newActorPoiGroups, oldActorGroup?.Enabled ?? false);
        AddFollowersIfMissing(new List<IPoiGrouper>
        {
            newActorGroupCollection
        });

        if (oldActorGroup == null)
        {
            _poiGroups.Add(newActorGroupCollection);
            return;
        }

        var oldPoiGroupsByTitle = oldActorGroup.PoiGroups
            .ToDictionary(group => group.BaseTitle);

        foreach (var poiGroup in newActorGroupCollection.PoiGroups)
        {
            if (oldPoiGroupsByTitle.GetValueOrDefault(poiGroup.BaseTitle) is { } existingOldGroupByTitle)
            {
                poiGroup.SetEnabledNoRefresh(existingOldGroupByTitle.Enabled);
            }

            foreach (var poi in poiGroup.Pois)
            {
                poi.ApplyFilter(MapFilter);
            }
        }

        if (SelectedPoi is ActorPoi)
        {
            SelectedPoi = null;
        }

        _poiGroups.SetItem(_poiGroups.IndexOf(oldActorGroup), newActorGroupCollection);
        var newPois = newActorPoiGroups.SelectMany(group => group.Pois).ToList();

        _poisAdded.RemoveWhere(poi => poi is ActorPoi);
        var addedPois = newPois.Where(poi => poi.Enabled && _poisAdded.Add(poi)).ToList();

        if (addedPois.Count > 0)
        {
            Pois.RemoveAndAdd(poi => poi is ActorPoi, addedPois);
        }
    }

    private void RefreshPois(PoiRefreshEvent message)
    {
        var pois = message.Grouper switch
        {
            PoiGroup poiGroup => poiGroup.Pois.Where(poi => poi.Enabled),
            PoiGroupCollection poiGroupCollection => poiGroupCollection.PoiGroups.SelectMany(group => group.Pois)
                .Where(poi => poi.Enabled),
            _ => null
        };

        if (pois != null)
        {
            AddPois(pois);
        }
    }

    private void AddPois(IEnumerable<IPoi> pois)
    {
        var toBeAdded = pois.Where(poi =>
        {
            if (_poisAdded.Contains(poi))
            {
                return false;
            }

            poi.ApplyFilter(MapFilter);
            _poisAdded.Add(poi);
            return true;
        }).ToList();

        if (toBeAdded.Count == 0)
        {
            return;
        }

        Pois.AddRange(toBeAdded);
    }

    [RelayCommand]
    private static void OpenCategorySelector()
    {
        WeakReferenceMessenger.Default.Send(new OpenCategorySelectorEvent());
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var poiGrouper in _poiGroups)
        {
            poiGrouper.SetEnabledNoRefresh(false);
        }
    }

    public void SaveSettings()
    {
        Settings.Default.MapType = MapSelection;

        GetSeletedPoiGroupsFromSettings(out var selected);

        foreach (var poiGrouper in _poiGroups)
        {
            switch (poiGrouper)
            {
                case PoiGroup poiGroup:
                    if (poiGroup.Enabled)
                    {
                        selected.Add(poiGroup.GroupKey);
                    }
                    else
                    {
                        selected.Remove(poiGroup.GroupKey);
                    }

                    break;
                case PoiGroupCollection poiGroupCollection:
                {
                    if (poiGroupCollection.Enabled)
                    {
                        selected.Add(poiGroupCollection.GroupKey);
                    }
                    else
                    {
                        selected.Remove(poiGroupCollection.GroupKey);
                    }

                    foreach (var group in poiGroupCollection.PoiGroups)
                    {
                        if (group.Enabled)
                        {
                            selected.Add(group.GroupKey);
                        }
                        else
                        {
                            selected.Remove(group.GroupKey);
                        }
                    }

                    break;
                }
            }
        }

        var selectedString = string.Join(SaveSelectedPoiGroupSeparator, selected);
        Settings.Default.SelectedMapGroups = selectedString;
        Settings.Default.Save();
    }

    private static void LoadSavedSelectedPois(List<IPoiGrouper> poiGrouperList)
    {
        if (!GetSeletedPoiGroupsFromSettings(out var selectedGroups))
        {
            return;
        }

        foreach (var poiGrouper in poiGrouperList)
        {
            switch (poiGrouper)
            {
                case PoiGroup poiGroup:
                    poiGroup.SetEnabledNoRefresh(selectedGroups.Contains(poiGroup.GroupKey), false);
                    break;
                case PoiGroupCollection poiGroupCollection:
                {
                    poiGroupCollection.SetEnabledNoRefresh(selectedGroups.Contains(poiGroupCollection.GroupKey), true);

                    foreach (var group in poiGroupCollection.PoiGroups)
                    {
                        group.SetEnabledNoRefresh(selectedGroups.Contains(group.GroupKey), false);
                    }

                    break;
                }
            }
        }
    }

    private static bool GetSeletedPoiGroupsFromSettings(out HashSet<string> selectedGroups)
    {
        var selectedGroupsString = Settings.Default.SelectedMapGroups;
        if (selectedGroupsString == "_null_")
        {
            selectedGroups = new HashSet<string>();
            return false;
        }

        if (string.IsNullOrEmpty(selectedGroupsString))
        {
            selectedGroups = new HashSet<string>();
        }
        else
        {
            var selectedGroupArray = selectedGroupsString.Split(SaveSelectedPoiGroupSeparator);
            selectedGroups = new HashSet<string>(selectedGroupArray);
        }

        return true;
    }
}