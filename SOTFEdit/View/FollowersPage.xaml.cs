using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
/// Interaction logic for UserControl1.xaml
/// </summary>
public partial class FollowersPage
{
    public FollowersPage()
    {
        DataContext = Ioc.Default.GetRequiredService<FollowerPageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        return Ioc.Default.GetRequiredService<FollowerPageViewModel>().Update(savegame, createBackup);
    }
}