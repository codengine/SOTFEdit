using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;

namespace SOTFEdit.Model.Map;

public partial class ZiplinePoi : ObservableObject, IPoi
{
    [NotifyPropertyChangedFor(nameof(Visible))]
    [ObservableProperty]
    private bool _enabled;

    [NotifyPropertyChangedFor(nameof(Visible))]
    [ObservableProperty]
    private bool _filtered;

    [ObservableProperty]
    private bool _isSelected;

    public ZiplinePoi(JToken token, Position posA, Position posB)
    {
        Token = token;
        PointA = new ZipPointPoi(posA, this);
        PointB = new ZipPointPoi(posB, this);
    }

    public JToken Token { get; set; }

    public double X1 => PointA.IconLeft + 16;
    public double X2 => PointB.IconLeft + 16;
    public double Y1 => PointA.IconTop;
    public double Y2 => PointB.IconTop;

    public ZipPointPoi PointA { get; }
    public ZipPointPoi PointB { get; }

    public bool Visible => _enabled && !Filtered;

    public Position? Position => null;
    public string? AreaName => null;
    public BitmapImage Icon => null!;
    public float IconLeft => 0;
    public float IconTop => 0;
    public string Title => "";
    public string Description => "";

    public int IconWidth => 0;
    public int IconHeight => 0;
    public int IconZIndex => -1;
    public float IconRotation => 0;

    public void ApplyFilter(MapFilter mapFilter)
    {
        Filtered = mapFilter.RequirementsFilter == MapFilter.RequirementsFilterType.InaccessibleOnly;
    }

    public void SetEnabledNoRefresh(bool value)
    {
        _enabled = value;
    }

    public string PrintableCoordinates => "";
    public bool IsUnderground => false;

    public void Refresh()
    {
        OnPropertyChanged(nameof(X1));
        OnPropertyChanged(nameof(X2));
        OnPropertyChanged(nameof(Y1));
        OnPropertyChanged(nameof(Y2));
    }
}