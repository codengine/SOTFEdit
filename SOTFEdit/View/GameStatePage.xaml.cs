using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for GameStatePage.xaml
/// </summary>
public partial class GameStatePage
{
    private readonly GameStatePageViewModel _dataContext;

    public GameStatePage()
    {
        DataContext = _dataContext = Ioc.Default.GetRequiredService<GameStatePageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame savegame)
    {
        return _dataContext.Update(savegame);
    }
}