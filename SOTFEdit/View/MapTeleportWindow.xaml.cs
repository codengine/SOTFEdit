using System.Windows;
using System.Windows.Input;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Map;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class MapTeleportWindow : ICloseable
{
    public MapTeleportWindow(Window owner, BasePoi destination,
        MapTeleportWindowViewModel.TeleportationMode teleportationMode)
    {
        Owner = owner;
        DataContext = new MapTeleportWindowViewModel(this, destination, teleportationMode);
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