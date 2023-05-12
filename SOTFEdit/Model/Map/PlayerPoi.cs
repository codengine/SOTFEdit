using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public class PlayerPoi : BasePoi
{
    private const string IconFile = "player.png";

    public PlayerPoi(Position position) : base(position)
    {
        PoiMessenger.Instance.Register<PlayerPosChangedEvent>(this,
            (_, message) => OnPlayerPosChangedEvent(message));
    }

    public override BitmapImage Icon => LoadBaseIcon(IconFile);

    public override int IconZIndex => 50;

    public override string Title => TranslationManager.Get("player.mapGroupName");
    public static BitmapImage IconSmall => LoadBaseIcon(IconFile, 24, 24);

    private void OnPlayerPosChangedEvent(PlayerPosChangedEvent message)
    {
        Position = message.NewPosition;
        PoiMessenger.Instance.Send(new ReapplyPoiFilterEvent(this));
    }
}