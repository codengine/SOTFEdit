using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;

namespace SOTFEdit.ViewModel;

public partial class MapViewModel : ObservableObject
{
    private readonly DispatcherTimer _fullTextFilterDispatcherTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(1000)
    };

    private readonly GameData _gameData;
    private readonly MapManager _mapManager;

    private readonly ObservableCollectionEx<IPoiGrouper> _poiGroups;

    private readonly HashSet<IPoi> _poisAdded = new();

    [ObservableProperty]
    private IPoi? _selectedPoi;

    public MapViewModel(IEnumerable<IPoiGrouper> pois, MapManager mapManager, GameData gameData)
    {
        _mapManager = mapManager;
        _gameData = gameData;
        _poiGroups = new ObservableCollectionEx<IPoiGrouper>(pois);

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

    public MapFilter MapFilter { get; }

    public ObservableCollectionEx<IPoi> Pois { get; }

    public ICollectionView PoiGroups { get; }

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
        }

        if (value is BasePoi basePoi)
        {
            basePoi.IsSelected = true;
        }
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
            (_, message) => { RefreshPois(message); });
        PoiMessenger.Instance.Register<NpcsReloadedEvent>(this,
            (_, _) => OnNpcsReloaded());
        PoiMessenger.Instance.Register<RemovePoiEvent>(this,
            (_, message) => OnRemovePoiEvent(message));
        PoiMessenger.Instance.Register<ReapplyPoiFilterEvent>(this, (_, message) => OnReapplyPoiFilterEvent(message));
        PoiMessenger.Instance.Register<WorldItemsChangedEvent>(this, (_, _) => OnWorldItemsChangedEvent());
    }

    private void OnWorldItemsChangedEvent()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var newWorldItemGroups = _mapManager.GetWorldItemPois(selectedSavegame, _gameData.Items)
            .Select(kvp =>
                new PoiGroup(false, kvp.Value, kvp.Key, kvp.Value.First().IconSmall, PoiGroupType.WorldItems))
            .ToList();

        var oldWorldItemGroups = _poiGroups.Where(group => group.PoiGroupType == PoiGroupType.WorldItems).ToList();

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
        var removed = message.Pois.Where(poi => _poisAdded.Remove(poi)).ToList();

        if (removed.Count > 0)
        {
            Pois.RemoveRange(removed);
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
    }

    private void OnNpcsReloaded()
    {
        var oldActorGroup = _poiGroups.OfType<PoiGroupCollection>()
            .FirstOrDefault(group => group.PoiGroupType == PoiGroupType.Actors);
        var newActorPoiGroups = _mapManager.GetActorPois();
        var newActorGroupCollection = PoiGroupCollection.ForActors(newActorPoiGroups, oldActorGroup?.Enabled ?? false);

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
}