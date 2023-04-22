using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;
using SOTFEdit.Model.SaveData.Actor;

namespace SOTFEdit.ViewModel;

public partial class MapSpawnActorsViewModel : ObservableObject
{
    private readonly BasePoi _destination;
    private readonly ICloseable _parent;

    [ObservableProperty]
    private int? _familyId;

    [NotifyCanExecuteChangedFor(nameof(DoSpawnCommand))]
    [ObservableProperty]
    private ActorType? _selectedActorType;

    [ObservableProperty]
    private Area _selectedArea;

    [ObservableProperty]
    private SpawnPattern _selectedSpawnPattern = SpawnPattern.Grid;

    [ObservableProperty]
    private int _spaceBetween;

    [NotifyCanExecuteChangedFor(nameof(DoSpawnCommand))]
    [ObservableProperty]
    private int _spawnCount = 1;

    [ObservableProperty]
    private float _xOffset = 1;

    [ObservableProperty]
    private float _yOffset = 1;

    [ObservableProperty]
    private float _zOffset = 1;

    public MapSpawnActorsViewModel(ICloseable parent, BasePoi destination, IList<int> allFamilyIds,
        List<ActorType> allActorTypes)
    {
        AllInfluences = Influence.AllTypes.Select(type =>
                new ComboBoxItemAndValue<string>(TranslationManager.Get("actors.influenceType." + type), type))
            .ToList();

        AllFamilyIds = new List<int>(allFamilyIds);
        allFamilyIds.Insert(0, -1);
        AllActorTypes = destination.IsUnderground
            ? allActorTypes.Where(actorType => !actorType.IsFollower()).ToList()
            : allActorTypes;
        _parent = parent;
        _destination = destination;
        X = destination.Position?.X ?? destination.X;
        Y = destination.Position?.Y ?? 0;
        Z = destination.Position?.Z ?? destination.Z;

        if (destination is ActorPoi actorPoi)
        {
            OriginalFamilyId = FamilyId = actorPoi.Actor.FamilyId;
            SelectedActorType = actorPoi.Actor.ActorType;
        }
        else
        {
            OriginalFamilyId = FamilyId = -1;
        }

        Areas = Ioc.Default.GetRequiredService<GameData>().AreaManager.GetAllAreas()
            .OrderBy(area => area.Name)
            .ToList();
        _selectedArea = destination.Position?.Area ?? AreaMaskManager.Surface;

        AllSpawnPatterns = Enum.GetValues<SpawnPattern>()
            .Select(pattern =>
                new ComboBoxItemAndValue<SpawnPattern>(
                    TranslationManager.Get($"map.spawnWindow.spawnPattern.{pattern}"),
                    pattern))
            .ToList();
    }

    public int? OriginalFamilyId { get; }

    public List<ComboBoxItemAndValue<string>> AllInfluences { get; }

    public List<int> AllFamilyIds { get; }
    public List<ActorType> AllActorTypes { get; }

    public ObservableCollection<Influence> Influences { get; } = new();

    public List<ComboBoxItemAndValue<SpawnPattern>> AllSpawnPatterns { get; }

    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public string Destination => _destination.Title;

    public List<Area> Areas { get; }

    private bool CanSpawn()
    {
        return SelectedActorType != null && SpawnCount > 0;
    }

    [RelayCommand]
    private void SetSpawnPattern(SpawnPattern spawnPattern)
    {
        SelectedSpawnPattern = spawnPattern;
    }

    [RelayCommand(CanExecute = nameof(CanSpawn))]
    private void DoSpawn()
    {
        if (SelectedActorType == null)
        {
            return;
        }

        var newPosition = new Position(X + XOffset, Y + YOffset, Z + ZOffset)
        {
            Area = _destination.Position?.Area ?? AreaMaskManager.Surface
        };

        PoiMessenger.Instance.Send(new SpawnActorsEvent(newPosition, SelectedActorType, SpawnCount,
            FamilyId == -1 ? null : FamilyId, new List<Influence>(Influences), SpaceBetween, SelectedSpawnPattern));

        _parent.Close();
    }
}

public enum SpawnPattern
{
    Grid,
    Random,
    Cross,
    VerticalLine,
    HorizontalLine,
    Rectangle
}