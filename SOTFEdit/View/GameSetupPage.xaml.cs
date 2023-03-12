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
        InitializeComponent();
    }

    public void Update(Savegame savegame, bool createBackup)
    {
        ((GameSetupPageViewModel)DataContext).Update(savegame, createBackup);
    }
}