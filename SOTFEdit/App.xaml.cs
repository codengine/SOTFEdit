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
using SOTFEdit.Model.Storage;
using SOTFEdit.ViewModel;

namespace SOTFEdit;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

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
        services.AddSingleton<StoragePageViewModel>();
        services.AddSingleton(_ => BuildGameDataInstance() ?? throw new Exception("Unable to load Game Data"));
        services.AddSingleton<StorageFactory>();
        services.AddSingleton<UpdateChecker>();
        services.AddSingleton<LabExperiments>();
        services.AddTransient<SelectSavegameViewModel>();
        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }

    private static GameData? BuildGameDataInstance()
    {
        var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "data.json"));
        return JsonConvert.DeserializeObject<GameData>(json);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
    }

    private void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

        DispatcherUnhandledException += (_, e) =>
        {
            LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
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