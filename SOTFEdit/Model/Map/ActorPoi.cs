using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public partial class ActorPoi : BasePoi
{
    private readonly bool _isFollower;

    public ActorPoi(Actor actor) : base(actor.Position)
    {
        Actor = actor;
        if (actor.TypeId is not (Constants.Actors.KelvinTypeId or Constants.Actors.VirginiaTypeId))
        {
            return;
        }

        PoiMessenger.Instance.Register<UpdateActorPoiPositionEvent>(this,
            (_, message) => OnUpdateActorPoiPositionEvent(message));
        _isFollower = true;
        SetEnabledNoRefresh(true);
    }

    public Actor Actor { get; }

    public override string Title => Actor.PrintableTitle;

    public override BitmapImage Icon =>
        Actor.ActorType?.IconPath is { } iconPath ? iconPath.LoadAppLocalImage() : DefaultIcon;

    public override int IconZIndex => _isFollower ? 50 : base.IconZIndex;

    private void OnUpdateActorPoiPositionEvent(UpdateActorPoiPositionEvent message)
    {
        if (message.TypeId == Actor.TypeId)
        {
            Position = message.NewPosition;
        }
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
    private void EditActor()
    {
        WeakReferenceMessenger.Default.Send(new RequestEditActorEvent(Actor));
    }
}