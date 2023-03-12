using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for UserControl1.xaml
/// </summary>
public partial class InventoryPage
{
    public InventoryPage()
    {
        InitializeComponent();
    }

    public void Update(Savegame savegame, bool createBackup)
    {
        ((InventoryPageViewModel)DataContext).Update(savegame, createBackup);
    }
}