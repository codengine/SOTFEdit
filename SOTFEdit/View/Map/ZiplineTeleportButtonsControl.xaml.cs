using System.Collections.Generic;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.View.Map;

[ObservableObject]
public partial class ZiplineTeleportButtonsControl
{
    public static readonly DependencyProperty PoiProperty = DependencyProperty.Register(
        nameof(Poi), typeof(ZipPointPoi), typeof(ZiplineTeleportButtonsControl),
        new PropertyMetadata(default(ZipPointPoi)));

    public ZiplineTeleportButtonsControl()
    {
        InitializeComponent();
        Unloaded += OnUnloaded;
    }

    public ZipPointPoi Poi
    {
        get => (ZipPointPoi)GetValue(PoiProperty);
        set => SetValue(PoiProperty, value);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        PoiMessenger.Instance.UnregisterAll(this);
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
        var parent = poi.Parent;
        if (ZiplineManager.Remove(parent, savegame))
        {
            PoiMessenger.Instance.Send(new RemovePoiEvent(new List<IPoi>
            {
                parent, parent.PointA, parent.PointB
            }));
        }
    }
}