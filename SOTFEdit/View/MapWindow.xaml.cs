using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.Infrastructure.Converters;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class MapWindow
{
    private readonly MapViewModel _dataContext;
    private IPoi? _clickedPoi;

    public MapWindow(RequestOpenMapEvent message)
    {
        DataContext = _dataContext = new MapViewModel(message.PoiGroups, Ioc.Default.GetRequiredService<MapManager>(),
            Ioc.Default.GetRequiredService<GameData>());
        _dataContext.IsNotConnected = !Ioc.Default.GetRequiredService<CompanionConnectionManager>().IsConnected();
        SetupListeners();
        InitializeComponent();
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<OpenCategorySelectorEvent>(this,
            (_, _) => OnOpenCategorySelectorEvent());
        WeakReferenceMessenger.Default.Register<ShowMapImageEvent>(this,
            (_, message) => OnShowMapImageEvent(message));
        PoiMessenger.Instance.Register<ShowTeleportWindowEvent>(this,
            (_, message) => OnShowTeleportWindowEvent(message));
        PoiMessenger.Instance.Register<ShowSpawnActorsWindowEvent>(this,
            (_, message) => OnShowSpawnActorsWindowEvent(message));
        PoiMessenger.Instance.Register<SpawnActorsEvent>(this,
            (_, message) => OnSpawnActorsEvent(message));
        PoiMessenger.Instance.Register<SelectedPoiChangedEvent>(this,
            (_, message) => OnSelectedPoiChangedEvent(message.IsSelected));
        PoiMessenger.Instance.Register<PlayerPosChangedEvent>(this,
            (_, message) => OnPlayerPosChangedEvent(message));
    }

    private void OnPlayerPosChangedEvent(PlayerPosChangedEvent message)
    {
        if (!_dataContext.FollowPlayer)
        {
            return;
        }

        var ingameToPixel = CoordinateConverter.IngameToPixel(message.NewPosition.X, message.NewPosition.Z);
        MapZoomControl.ZoomToPos(ingameToPixel.Item1, ingameToPixel.Item2, -16);
    }

    private void OnSelectedPoiChangedEvent(bool isSelected)
    {
        PoiDetailsFlyout.IsOpen = isSelected;
    }

    private static void OnSpawnActorsEvent(SpawnActorsEvent message)
    {
        if (SavegameManager.SelectedSavegame is { } selectedSavegame)
        {
            ActorModifier.Spawn(selectedSavegame, message.Position, message.ActorType, message.SpawnCount,
                message.FamilyId, message.Influences, message.SpaceBetween, message.SpawnPattern);
        }
    }

    private void OnShowSpawnActorsWindowEvent(ShowSpawnActorsWindowEvent message)
    {
        var window = new MapSpawnActorsWindow(this, message.Poi);
        window.ShowDialog();
    }

    private void OnShowTeleportWindowEvent(ShowTeleportWindowEvent message)
    {
        var window = new MapTeleportWindow(this, message.Destination, message.TeleportationMode);
        window.ShowDialog();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        WeakReferenceMessenger.Default.UnregisterAll(DataContext);
        PoiMessenger.Instance.Reset();
        _dataContext.SaveSettings();
    }

    private void OnShowMapImageEvent(ShowMapImageEvent message)
    {
        var window = new ShowImageWindow(this, message.Url, message.Title);
        window.ShowDialog();
    }

    private void OnOpenCategorySelectorEvent()
    {
        MapOptionsFlyout.IsOpen = !MapOptionsFlyout.IsOpen;
    }

    private void PoiSelector_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scv = (ScrollViewer)sender;
        scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
        e.Handled = true;
    }

    private void ZoomControl_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ZoomControl.ZoomControl zoomControl)
        {
            return;
        }

        if (zoomControl.IsPanning)
        {
            _dataContext.FollowPlayer = false;
            return;
        }

        if (_clickedPoi is { } clickedPoi)
        {
            if (_dataContext.SelectedPoi is IClickToMovePoi { IsMoveRequested: true } clickToMovePoi)
            {
                if (clickedPoi.Position is { } position)
                {
                    clickToMovePoi.AcceptNewPos(position);
                }
            }
            else
            {
                _dataContext.SelectedPoi = clickedPoi;
            }

            _clickedPoi = null;
        }
        else
        {
            if (_dataContext.SelectedPoi is IClickToMovePoi clickToMovePoi)
            {
                clickToMovePoi.IsMoveRequested = false;
            }

            _dataContext.SelectedPoi = null;
        }
    }

    private void sotfLink_Click(object sender, MouseButtonEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForUrl("https://sotf.th.gl/"));
    }

    private void MapWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }

    private void ZoomControl_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        _clickedPoi = e.OriginalSource is Image { Tag: IPoi ipoi } ? ipoi : null;
    }

    private void AlwaysOnTop_OnChecked(object sender, RoutedEventArgs e)
    {
        Topmost = true;
    }

    private void AlwaysOnTop_OnUnchecked(object sender, RoutedEventArgs e)
    {
        Topmost = false;
    }
}