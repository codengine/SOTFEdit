using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Infrastructure;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.Model;
using SOTFEdit.Model.Map;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class MapTeleportWindow : ICloseable
{
    public MapTeleportWindow(Window owner, BasePoi destination,
        MapTeleportWindowViewModel.TeleportationMode teleportationMode)
    {
        Owner = owner;
        var companionConnectionManager = Ioc.Default.GetRequiredService<CompanionConnectionManager>();
        var gameData = Ioc.Default.GetRequiredService<GameData>();
        DataContext = new MapTeleportWindowViewModel(this, destination, teleportationMode, companionConnectionManager,
            gameData.AreaManager);
        InitializeComponent();
    }

    private void MapTeleportWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}