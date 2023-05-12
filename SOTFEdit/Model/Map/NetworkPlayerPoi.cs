using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public partial class NetworkPlayerPoi : BasePoi
{
    private const string IconFile = "network_player.png";

    [ObservableProperty]
    private string _title;

    public NetworkPlayerPoi(Position position, int instanceId, string name) : base(position)
    {
        InstanceId = instanceId;
        _title = name;
        PoiMessenger.Instance.Register<NetworkPlayerPosChangedEvent>(this,
            (_, message) => OnNetworkPlayerPosChangedEvent(message));
    }

    public int InstanceId { get; }

    public override BitmapImage Icon => LoadBaseIcon(IconFile);

    public override int IconZIndex => 50;

    private void OnNetworkPlayerPosChangedEvent(NetworkPlayerPosChangedEvent message)
    {
        if (message.InstanceId != InstanceId)
        {
            return;
        }

        Position = message.NewPosition;
        Title = message.Name ?? "???";
        PoiMessenger.Instance.Send(new ReapplyPoiFilterEvent(this));
    }
}