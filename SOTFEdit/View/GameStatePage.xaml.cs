using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.ViewModel;
using SOTFEdit.Model;

namespace SOTFEdit.View;

/// <summary>
/// Interaction logic for GameStatePage.xaml
/// </summary>
public partial class GameStatePage
{
    public GameStatePage()
    {
        DataContext = Ioc.Default.GetRequiredService<GameStatePageViewModel>();
        InitializeComponent();
    }

    public void Update(Savegame savegame, bool createBackup)
    {
        ((GameStatePageViewModel)DataContext).Update(savegame, createBackup);
    }
}