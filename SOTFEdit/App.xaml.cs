using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public App()
    {
        SetupExceptionHandling();

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
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ArmorPageViewModel>();
        services.AddSingleton<FollowerPageViewModel>();
        services.AddSingleton<GameSetupPageViewModel>();
        services.AddSingleton<GameStatePageViewModel>();
        services.AddSingleton<InventoryPageViewModel>();
        services.AddSingleton<PlayerPageViewModel>();
        services.AddSingleton<WeatherPageViewModel>();
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

    private void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

        DispatcherUnhandledException += (s, e) =>
        {
            LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        };
    }

    private static void LogUnhandledException(Exception exception, string source)
    {
        var message = $"Unhandled exception ({source})";
        try
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            message = $"Unhandled exception in {assemblyName.Name} v{assemblyName.Version}";
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exception in LogUnhandledException");
        }
        finally
        {
            Logger.Error(exception, message);
        }
    }
}