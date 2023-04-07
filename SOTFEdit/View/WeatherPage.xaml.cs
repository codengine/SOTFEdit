using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for WeatherPage.xaml
/// </summary>
public partial class WeatherPage
{
    private readonly WeatherPageViewModel _dataContext;

    public WeatherPage()
    {
        DataContext = _dataContext = Ioc.Default.GetRequiredService<WeatherPageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame selectedSavegame)
    {
        return _dataContext.Update(selectedSavegame);
    }
}