using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public partial class WorldItemPoi : BasePoi
{
    private readonly Item? _item;

    public WorldItemPoi(WorldItemState state, Item? item) : base(state.Position)
    {
        State = state;
        _item = item;
    }

    public WorldItemState State { get; }

    public override BitmapImage Icon => GetIcon();
    public BitmapImage IconSmall => GetIcon(24, 24);

    public override string Title => _item != null ? _item.Name :
        string.IsNullOrWhiteSpace(State.Group) ? State.ObjectNameId : State.Group;

    public string? WikiLink => _item?.Wiki;

    private BitmapImage GetIcon(int? width = null, int? height = null)
    {
        return State.ItemId switch
        {
            626 => "/images/worldobjects/hang-gliding.png".LoadAppLocalImage(width, height),
            630 => "/images/worldobjects/monowheel.png".LoadAppLocalImage(width, height),
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

    [RelayCommand]
    private void OpenWiki()
    {
        if (WikiLink != null)
        {
            WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForUrl(WikiLink));
        }
    }
}