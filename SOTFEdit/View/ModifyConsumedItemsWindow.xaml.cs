using System.Windows;
using System.Windows.Input;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class ModifyConsumedItemsWindow : ICloseable
{
    public ModifyConsumedItemsWindow(Window owner, Savegame savegame, GameData gameData)
    {
        Owner = owner;
        DataContext = new ModifyConsumedItemsViewModel(savegame, gameData, this);
        InitializeComponent();
    }

    private void ModifyConsumedItemsWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}