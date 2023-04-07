using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for GameSetupPage.xaml
/// </summary>
public partial class GameSetupPage
{
    private readonly GameSetupPageViewModel _dataContext;

    public GameSetupPage()
    {
        DataContext = _dataContext = Ioc.Default.GetRequiredService<GameSetupPageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame savegame)
    {
        return _dataContext.Update(savegame);
    }
}