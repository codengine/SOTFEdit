using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.ViewModel;
using SOTFEdit.Model;

namespace SOTFEdit.View;

/// <summary>
/// Interaction logic for PlayerPage.xaml
/// </summary>
public partial class PlayerPage
{
    public PlayerPage()
    {
        DataContext = Ioc.Default.GetRequiredService<PlayerPageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        return Ioc.Default.GetRequiredService<PlayerPageViewModel>().Update(savegame, createBackup);
    }
}