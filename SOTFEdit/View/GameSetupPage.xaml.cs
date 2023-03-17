using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for GameSetupPage.xaml
/// </summary>
public partial class GameSetupPage
{
    public GameSetupPage()
    {
        DataContext = Ioc.Default.GetRequiredService<GameSetupPageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        return Ioc.Default.GetRequiredService<GameSetupPageViewModel>().Update(savegame, createBackup);
    }
}