using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Map;

namespace SOTFEdit.ViewModel;

public partial class MapTeleportWindowViewModel : ObservableObject
{
    public enum TeleportationMode
    {
        Player,
        Kelvin,
        Virginia
    }

    private readonly BasePoi _destination;

    private readonly ICloseable _parent;
    private readonly TeleportationMode _teleportationMode;

    [ObservableProperty]
    private Area _selectedArea;

    [ObservableProperty]
    private float _xOffset = 1;

    [ObservableProperty]
    private float _yOffset = 1;

    [ObservableProperty]
    private float _zOffset = 1;

    public MapTeleportWindowViewModel(ICloseable parent, BasePoi destination, TeleportationMode teleportationMode)
    {
        _parent = parent;
        _destination = destination;
        _teleportationMode = teleportationMode;
        X = destination.Position?.X ?? destination.X;
        Y = destination.Position?.Y ?? 0;
        Z = destination.Position?.Z ?? destination.Z;
        Areas = Ioc.Default.GetRequiredService<GameData>().AreaManager.GetAllAreas()
            .OrderBy(area => area.Name)
            .ToList();
        _selectedArea = destination.Position?.Area ?? AreaMaskManager.Surface;
    }

    public List<Area> Areas { get; }

    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public string Target => TranslationManager.Get("map.teleportWindow.mode." + _teleportationMode);
    public string Destination => _destination.Title;

    [RelayCommand]
    private void DoTeleport()
    {
        var newPosition = new Position(X + XOffset, Y + YOffset, Z + ZOffset)
        {
            Area = SelectedArea
        };

        switch (_teleportationMode)
        {
            case TeleportationMode.Player:
                TeleportPlayer(newPosition);
                break;
            case TeleportationMode.Kelvin:
                TeleportKelvin(newPosition);
                break;
            case TeleportationMode.Virginia:
                TeleportVirginia(newPosition);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _parent.Close();
    }

    private static void TeleportPlayer(Position newPosition)
    {
        var playerState = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState;
        playerState.Pos = newPosition;
    }

    private static void TeleportKelvin(Position newPosition)
    {
        var playerState = Ioc.Default.GetRequiredService<FollowerPageViewModel>().KelvinState;
        TeleportFollower(playerState, newPosition);
    }

    private static void TeleportVirginia(Position newPosition)
    {
        var playerState = Ioc.Default.GetRequiredService<FollowerPageViewModel>().VirginiaState;
        TeleportFollower(playerState, newPosition);
    }

    private static void TeleportFollower(FollowerState followerState, Position newPosition)
    {
        followerState.Pos = newPosition;
    }
}