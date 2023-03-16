using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
/// Interaction logic for WeatherPage.xaml
/// </summary>
public partial class WeatherPage
{
    public WeatherPage()
    {
        DataContext = Ioc.Default.GetRequiredService<WeatherPageViewModel>();
        InitializeComponent();
    }

    public bool Update(Savegame selectedSavegame, bool createBackup)
    {
        return ((WeatherPageViewModel)DataContext).Update(selectedSavegame, createBackup);
    }
}