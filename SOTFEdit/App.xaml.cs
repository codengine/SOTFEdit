using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public App()
    {
        if (Settings.Default.UpgradeRequired)
        {
            Settings.Default.Upgrade();
            Settings.Default.UpgradeRequired = false;
            Settings.Default.Save();
        }

        ConfigureServices();
        InitializeComponent();
    }

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new SavegameManager());
        services.AddTransient<MainViewModel>();
        services.AddSingleton<GameSetupPageViewModel>();
        services.AddSingleton<InventoryPageViewModel>();
        services.AddSingleton(_ => BuildItemListInstance());
        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }

    private static ItemList BuildItemListInstance()
    {
        var json = File.ReadAllText(@"items.json");
        var items = JsonConvert.DeserializeObject<Item[]>(json);
        return items != null ? new ItemList(items) : new ItemList();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
    }
}