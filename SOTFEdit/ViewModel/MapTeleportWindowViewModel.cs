using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Companion.Shared.Messages;
using SOTFEdit.Infrastructure;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.Model;
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

    private readonly AreaMaskManager _areaMaskManager;

    private readonly CompanionConnectionManager _companionConnectionManager;

    private readonly BasePoi _destination;

    private readonly ICloseable _parent;
    private readonly TeleportationMode _teleportationMode;

    [ObservableProperty]
    private Area _selectedArea;

    [ObservableProperty]
    private float _xOffset;

    [ObservableProperty]
    private float _yOffset;

    [ObservableProperty]
    private float _zOffset;

    public MapTeleportWindowViewModel(ICloseable parent, BasePoi destination, TeleportationMode teleportationMode,
        CompanionConnectionManager companionConnectionManager, AreaMaskManager areaMaskManager)
    {
        _parent = parent;
        _destination = destination;
        _teleportationMode = teleportationMode;
        _companionConnectionManager = companionConnectionManager;
        _areaMaskManager = areaMaskManager;
        X = destination.Position?.X ?? destination.X;
        Y = destination.Position?.Y ?? 0;
        Z = destination.Position?.Z ?? destination.Z;

        destination.GetTeleportationOffset(out var xOffset, out var yOffset, out var zOffset);
        XOffset = xOffset;
        YOffset = yOffset;
        ZOffset = zOffset;

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

    private void TeleportPlayer(Position newPosition)
    {
        if (_companionConnectionManager.IsConnected())
        {
            var teleport = new CompanionTeleportMessage
            {
                Target = CharacterTarget.Player,
                AreaMask = newPosition.Area.AreaMask,
                GraphMask = _areaMaskManager.GetAreaForAreaMask(newPosition.Area.AreaMask).GraphMask,
                X = newPosition.X,
                Y = newPosition.Y,
                Z = newPosition.Z
            };
            _companionConnectionManager.SendAsync(teleport);
        }
        else
        {
            var playerState = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState;
            playerState.Pos = newPosition;
        }
    }

    private void TeleportKelvin(Position newPosition)
    {
        if (_companionConnectionManager.IsConnected())
        {
            TeleportFollowerRemotely(CharacterTarget.Kelvin, newPosition);
        }
        else
        {
            var followerState = Ioc.Default.GetRequiredService<FollowerPageViewModel>().KelvinState;
            followerState.Pos = newPosition;
        }
    }

    private void TeleportVirginia(Position newPosition)
    {
        if (_companionConnectionManager.IsConnected())
        {
            TeleportFollowerRemotely(CharacterTarget.Virginia, newPosition);
        }
        else
        {
            var followerState = Ioc.Default.GetRequiredService<FollowerPageViewModel>().VirginiaState;
            followerState.Pos = newPosition;
        }
    }

    private void TeleportFollowerRemotely(CharacterTarget characterTarget, Position newPosition)
    {
        var teleport = new CompanionTeleportMessage
        {
            Target = characterTarget,
            AreaMask = newPosition.Area.AreaMask,
            GraphMask = _areaMaskManager.GetAreaForAreaMask(newPosition.Area.AreaMask).GraphMask,
            X = newPosition.X,
            Y = newPosition.Y,
            Z = newPosition.Z
        };
        _companionConnectionManager.SendAsync(teleport);
    }
}