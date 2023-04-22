using System.Windows.Media.Imaging;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Map;

public class ZipPointPoi : BasePoi
{
    private const string IconFile = "pole.png";

    public ZipPointPoi(Position position, ZiplinePoi parent, bool isEndpoint) : base(position)
    {
        Parent = parent;
        IsEndpoint = isEndpoint;
    }

    public ZiplinePoi Parent { get; }
    public bool IsEndpoint { get; }

    public override BitmapImage Icon => LoadBaseIcon(IconFile);

    public static BitmapImage IconSmall => LoadBaseIcon(IconFile, 24, 24);

    public override int IconZIndex => -1;

    public override string Title => TranslationManager.Get("map.zipLineAnchor");

    public override void ApplyFilter(MapFilter mapFilter)
    {
        Filtered = ShouldFilter(mapFilter);
    }

    protected override bool ShouldFilter(MapFilter mapFilter)
    {
        return mapFilter.RequirementsFilter == MapFilter.RequirementsFilterType.InaccessibleOnly ||
               base.ShouldFilter(mapFilter);
    }
}