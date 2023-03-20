using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for StoragePage.xaml
/// </summary>
public partial class StoragePage
{
    private readonly StoragePageViewModel _dataContext;

    public StoragePage()
    {
        DataContext = _dataContext = Ioc.Default.GetRequiredService<StoragePageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        return _dataContext.Update(savegame, createBackup);
    }
}