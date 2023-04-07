using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for PlayerPage.xaml
/// </summary>
public partial class PlayerPage
{
    private readonly PlayerPageViewModel _dataContext;

    public PlayerPage()
    {
        DataContext = _dataContext = Ioc.Default.GetRequiredService<PlayerPageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame savegame)
    {
        return _dataContext.Update(savegame);
    }
}