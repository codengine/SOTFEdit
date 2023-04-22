using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Infrastructure;

public partial class ZoomHandler : ObservableObject
{
    [ObservableProperty]
    private double _zoom;

    [ObservableProperty]
    private double _zoomInverse;

    /// <summary>
    ///     Initializes a new instance of the ZoomHandler class with a default Zoom value of 1.0.
    /// </summary>
    public ZoomHandler()
    {
        Zoom = 1.0;
    }

    partial void OnZoomChanged(double value)
    {
        ZoomInverse = value == 0 ? 0 : 1 / value;
    }
}