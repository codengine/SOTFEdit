using SOTFEdit.Model.Map;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Events;

public class ShowTeleportWindowEvent
{
    public ShowTeleportWindowEvent(BasePoi destination, MapTeleportWindowViewModel.TeleportationMode teleportationMode)
    {
        Destination = destination;
        TeleportationMode = teleportationMode;
    }

    public BasePoi Destination { get; }
    public MapTeleportWindowViewModel.TeleportationMode TeleportationMode { get; }
}