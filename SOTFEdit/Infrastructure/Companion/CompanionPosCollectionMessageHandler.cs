using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Companion.Shared.Messages;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Infrastructure.Companion;

public class CompanionPosCollectionMessageHandler : MessageHandler<CompanionPosCollectionMessage>
{
    private readonly AreaMaskManager _areaMaskManager;
    private readonly FollowerPageViewModel _followerPageViewModel;
    private readonly PlayerPageViewModel _playerPageViewModel;

    public CompanionPosCollectionMessageHandler(GameData gameData, PlayerPageViewModel playerPageViewModel,
        FollowerPageViewModel followerPageViewModel)
    {
        _playerPageViewModel = playerPageViewModel;
        _followerPageViewModel = followerPageViewModel;
        _areaMaskManager = gameData.AreaManager;
    }

    protected override void Handle(CompanionPosCollectionMessage message)
    {
        Application.Current.Dispatcher.Invoke(DispatcherPriority.Render,
            () =>
            {
                foreach (var position in message.Positions)
                {
                    switch (position.Target)
                    {
                        case CharacterTarget.Player:
                            HandlePlayerPos(position);
                            break;
                        case CharacterTarget.Kelvin or CharacterTarget.Virginia:
                            HandleFollowerPos(position);
                            break;
                        case CharacterTarget.NetworkPlayer:
                            HandleNetworkPlayerPos(position);
                            break;
                    }
                }
            });
    }

    private void HandleNetworkPlayerPos(CompanionPosMessage position)
    {
        var newPos = new Position(position.X, position.Y, position.Z)
        {
            Area = _areaMaskManager.GetAreaForAreaMask(position.Mask),
            Rotation = position.Rotation
        };
        PoiMessenger.Instance.Send(new NetworkPlayerPosChangedEvent(position.InstanceId, position.Name, newPos));
    }

    private void HandleFollowerPos(CompanionPosMessage position)
    {
        var newPos = new Position(position.X, position.Y, position.Z)
        {
            Area = _areaMaskManager.GetAreaForGraphMask(position.Mask),
            Rotation = position.Rotation
        };

        if (position.Target == CharacterTarget.Kelvin)
        {
            _followerPageViewModel.KelvinState.Pos = newPos;
        }
        else
        {
            _followerPageViewModel.VirginiaState.Pos = newPos;
        }
    }

    private void HandlePlayerPos(CompanionPosMessage position)
    {
        var newPos = new Position(position.X, position.Y, position.Z)
        {
            Area = _areaMaskManager.GetAreaForAreaMask(position.Mask),
            Rotation = position.Rotation
        };
        _playerPageViewModel.PlayerState.Pos = newPos;
    }
}