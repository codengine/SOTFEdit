using System.Windows.Controls;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
/// Interaction logic for UserControl1.xaml
/// </summary>
public partial class FollowersPage : UserControl
{
    public FollowersPage()
    {
        InitializeComponent();
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        return ((FollowerPageViewModel)DataContext).Update(savegame, createBackup);
    }
}