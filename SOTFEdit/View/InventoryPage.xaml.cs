using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for UserControl1.xaml
/// </summary>
public partial class InventoryPage
{
    private readonly InventoryPageViewModel _dataContext;

    public InventoryPage()
    {
        DataContext = _dataContext = Ioc.Default.GetRequiredService<InventoryPageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        return _dataContext.Update(savegame, createBackup);
    }
}