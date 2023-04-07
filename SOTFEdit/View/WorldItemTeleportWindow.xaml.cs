using System.Windows;
using System.Windows.Input;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class WorldItemTeleportWindow : ICloseable
{
    public WorldItemTeleportWindow(Window owner, GameData gameData, Savegame savegame)
    {
        Owner = owner;
        DataContext = new WorldItemTeleporterViewModel(gameData, savegame, this);
        InitializeComponent();
    }

    private void WorldItemTeleportWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}