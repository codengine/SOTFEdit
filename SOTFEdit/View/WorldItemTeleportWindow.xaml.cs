using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class WorldItemTeleportWindow : ICloseableWithResult
{
    public WorldItemTeleportWindow(Window owner, Savegame savegame)
    {
        Owner = owner;
        DataContext = new WorldItemTeleporterViewModel(savegame, this);
        InitializeComponent();
    }

    public void Close(bool hasChanges)
    {
        base.Close();
        if (hasChanges)
        {
            PoiMessenger.Instance.Send(new WorldItemsChangedEvent());
        }
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