using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View.Map;

[ObservableObject]
public partial class ZiplineTeleportButtonsControl
{
    public static readonly DependencyProperty PoiProperty = DependencyProperty.Register(
        nameof(Poi), typeof(ZipPointPoi), typeof(ZiplineTeleportButtonsControl),
        new PropertyMetadata(default(ZipPointPoi)));

    private Position? _playerPos;

    public ZiplineTeleportButtonsControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public ZipPointPoi Poi
    {
        get => (ZipPointPoi)GetValue(PoiProperty);
        set => SetValue(PoiProperty, value);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _playerPos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos;
        MoveToPlayerCommand.NotifyCanExecuteChanged();

        PoiMessenger.Instance.Register<PlayerPosChangedEvent>(this, (_, message) =>
        {
            _playerPos = message.NewPosition;
            MoveToPlayerCommand.NotifyCanExecuteChanged();
        });
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        PoiMessenger.Instance.UnregisterAll(this);
    }

    private bool CanMoveToPlayer()
    {
        return !_playerPos?.Area.IsUnderground() ?? false;
    }

    [RelayCommand(CanExecute = nameof(CanMoveToPlayer))]
    private void MoveToPlayer()
    {
        if (_playerPos is not null && !_playerPos.Area.IsUnderground() &&
            SavegameManager.SelectedSavegame is { } selectedSavegame)
        {
            ModifyZipline(Poi, _playerPos, selectedSavegame);
        }
    }

    private static void ModifyZipline(ZipPointPoi poi, Position playerPos, Savegame savegame)
    {
        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ZipLineManagerSaveData) is
                not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ZipLineManager)?["Ziplines"] is not { } ziplines)
        {
            return;
        }

        foreach (var zipline in ziplines)
        {
            var parent = poi.Parent;
            if (!zipline.Equals(parent.Token))
            {
                continue;
            }

            var clonedZipline = zipline.DeepClone();
            clonedZipline["_anchorAPosition"] = !poi.IsEndpoint
                ? JToken.FromObject(playerPos)
                : JToken.FromObject(parent.PointA.Position!);
            clonedZipline["_anchorBPosition"] = poi.IsEndpoint
                ? JToken.FromObject(playerPos)
                : JToken.FromObject(parent.PointB.Position!);
            parent.Token = clonedZipline;
            zipline.Replace(clonedZipline);
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.ZipLineManager);
            poi.Position = playerPos;
            parent.Refresh();
            break;
        }
    }

    [RelayCommand]
    private void DeleteBothAnchors()
    {
        if (SavegameManager.SelectedSavegame is { } selectedSavegame)
        {
            DeleteBothAnchors(Poi, selectedSavegame);
        }
    }

    private static void DeleteBothAnchors(ZipPointPoi poi, Savegame savegame)
    {
        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ZipLineManagerSaveData) is
                not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ZipLineManager)?["Ziplines"] is not { } ziplines)
        {
            return;
        }

        var parent = poi.Parent;

        var toBeRemoved = ziplines.FirstOrDefault(zipline => zipline.Equals(parent.Token));

        if (toBeRemoved == null)
        {
            return;
        }

        toBeRemoved.Remove();
        PoiMessenger.Instance.Send(new RemovePoiEvent(new List<IPoi>
        {
            parent, parent.PointA, parent.PointB
        }));
    }
}