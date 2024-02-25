using System.Windows.Media.Imaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.WorldItem;

namespace SOTFEdit.Model.Map;

public class WorldItemPoi : BasePoi
{
    public WorldItemPoi(WorldItemState state) : base(state.Position)
    {
        State = state;
    }

    public WorldItemState State { get; }

    public override BitmapImage Icon => GetIcon();
    public BitmapImage IconSmall => GetIcon(24, 24);

    public override string Title => State.ObjectName;

    private BitmapImage GetIcon(int? width = null, int? height = null)
    {
        return State.WorldItemType switch
        {
            WorldItemType.HangGlider => "/images/worldobjects/hang-gliding.png".LoadAppLocalImage(width, height),
            WorldItemType.KnightV => "/images/worldobjects/monowheel.png".LoadAppLocalImage(width, height),
            WorldItemType.GolfCart => "/images/worldobjects/golf-cart.png".LoadAppLocalImage(width, height),
            WorldItemType.Radio => "/images/worldobjects/radio.png".LoadAppLocalImage(width, height),
            _ => DefaultIcon
        };
    }

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