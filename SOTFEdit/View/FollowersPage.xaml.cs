using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for UserControl1.xaml
/// </summary>
public partial class FollowersPage
{
    private readonly FollowerPageViewModel _dataContext;

    public FollowersPage()
    {
        DataContext = _dataContext = Ioc.Default.GetRequiredService<FollowerPageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame savegame)
    {
        return _dataContext.Update(savegame);
    }
}