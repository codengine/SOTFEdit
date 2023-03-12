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

    public void Update(Savegame selectedSavegame, bool createBackup)
    {
        ((WeatherPageViewModel)DataContext).Update(selectedSavegame, createBackup);
    }
}