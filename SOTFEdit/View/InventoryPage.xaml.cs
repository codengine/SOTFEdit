using CommunityToolkit.Mvvm.DependencyInjection;
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
        DataContext = Ioc.Default.GetRequiredService<InventoryPageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        return Ioc.Default.GetRequiredService<InventoryPageViewModel>().Update(savegame, createBackup);
    }
}