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

    public bool Update(Savegame savegame, bool createBackup)
    {
        return ((GameSetupPageViewModel)DataContext).Update(savegame, createBackup);
    }
}