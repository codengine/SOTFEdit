using SOTFEdit.Model.Map;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Events;

public class ShowTeleportWindowEvent(
    BasePoi destination, MapTeleportWindowViewModel.TeleportationMode teleportationMode)
{
    public BasePoi Destination { get; } = destination;
    public MapTeleportWindowViewModel.TeleportationMode TeleportationMode { get; } = teleportationMode;
}