using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public partial class ZipPointPoi : BasePoi, IClickToMovePoi
{
    private const string IconFile = "pole.png";

    [ObservableProperty]
    private bool _isMoveRequested;

    [ObservableProperty]
    private bool _isZiplineCreationRequested;

    public ZipPointPoi(Position position, ZiplinePoi parent) : base(position)
    {
        Parent = parent;
    }

    public ZiplinePoi Parent { get; }

    public override BitmapImage Icon => LoadBaseIcon(IconFile);

    public static BitmapImage IconSmall => LoadBaseIcon(IconFile, 24, 24);

    public override int IconZIndex => -1;

    public override string Title => TranslationManager.Get("map.zipLineAnchor");

    public void AcceptNewPos(Position newPosition)
    {
        if (IsZiplineCreationRequested)
        {
            IsMoveRequested = false;
            IsZiplineCreationRequested = false;
            if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
            {
                return;
            }

            var newToken = ZiplineManager.CreateNew(selectedSavegame, Position!, newPosition);
            if (newToken != null)
            {
                PoiMessenger.Instance.Send(new AddZipPoiEvent(new ZiplinePoi(newToken, Position!, newPosition)));
            }
        }
        else
        {
            IsMoveRequested = false;

            if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
            {
                return;
            }

            var posAOld = Parent.PointA.Position;
            var posBOld = Parent.PointB.Position;
            Position = newPosition;
            var posANew = Parent.PointA.Position;
            var posBNew = Parent.PointB.Position;

            var newToken = ZiplineManager.Move(selectedSavegame, Parent.Token, posAOld, posBOld, posANew, posBNew);
            if (newToken == null)
            {
                Parent.PointA.Position = posAOld;
                Parent.PointB.Position = posBOld;
            }
            else
            {
                Parent.Token = newToken;
            }

            Parent.Refresh();
        }
    }

    partial void OnIsMoveRequestedChanged(bool value)
    {
        _isZiplineCreationRequested = false;
        OnPropertyChanged(nameof(IsZiplineCreationRequested));
    }

    partial void OnIsZiplineCreationRequestedChanging(bool value)
    {
        _isMoveRequested = value;
        OnPropertyChanged(nameof(IsMoveRequested));
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