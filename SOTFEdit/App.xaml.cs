using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog;
using Semver;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Infrastructure;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;
using SOTFEdit.Model.Map.Static;
using SOTFEdit.Model.Storage;
using SOTFEdit.View;
using SOTFEdit.ViewModel;

namespace SOTFEdit;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public const string Version = "1.1.0";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public App()
    {
        InitializeLanguage();

        Logger.Debug("Initializing Application");
        SetupExceptionHandling();

        if (Settings.Default.UpgradeRequired)
        {
            Logger.Info("Upgrading Settings");
            Settings.Default.Upgrade();
            Settings.Default.UpgradeRequired = false;
            Settings.Default.Save();
        }

        MessagePackInitializer.Initialize();
        ConfigureServices();

        Logger.Debug("Initializing Component");
        InitializeComponent();
    }

    private static void InitializeLanguage()
    {
        var language = string.IsNullOrEmpty(Settings.Default.Language)
            ? CultureInfo.CurrentCulture.TwoLetterISOLanguageName
            : Settings.Default.Language;

        var availableCultures = LanguageManager.GetAvailableCultures().ToHashSet();
        if (!availableCultures.Contains(language))
        {
            language = "en";
        }

        var cultureInfo = new CultureInfo(language);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }

    private static void ConfigureServices()
    {
        Logger.Debug("Configuring Services");
        var services = new ServiceCollection();
        services.AddSingleton(new SavegameManager());
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ArmorPageViewModel>();
        services.AddSingleton<FollowerPageViewModel>();
        services.AddSingleton<GameSetupPageViewModel>();
        services.AddSingleton<GameStatePageViewModel>();
        services.AddSingleton<InventoryPageViewModel>();
        services.AddSingleton<GamePageViewModel>();
        services.AddSingleton<PlayerPageViewModel>();
        services.AddSingleton<WeatherPageViewModel>();
        services.AddSingleton<StoragePageViewModel>();
        services.AddSingleton(_ => BuildGameDataInstance() ?? throw new Exception("Unable to load Game Data"));
        services.AddSingleton<StorageFactory>();
        services.AddSingleton<UpdateChecker>();
        services.AddTransient<SelectSavegameViewModel>();
        services.AddSingleton<GamePage>();
        services.AddSingleton<GameStatePage>();
        services.AddSingleton<GameSetupPage>();
        services.AddSingleton<WeatherPage>();
        services.AddSingleton<NpcsPageViewModel>();
        services.AddSingleton<NpcsPage>();
        services.AddSingleton<ActorModifier>();
        services.AddSingleton<ApplicationSettings>();
        services.AddSingleton<StructuresPageViewModel>();
        services.AddSingleton<StructuresPage>();
        services.AddSingleton<MapManager>();
        services.AddSingleton<CompanionPoiStorage>();
        services.AddSingleton<IMessageHandler, CompanionAddPoiMessageHandler>();
        services.AddSingleton<IMessageHandler, CompanionPosCollectionMessageHandler>();
        services.AddSingleton<IMessageHandler, CompanionRequestPoiUpdateMessageHandler>();
        var networkPlayerManager = new CompanionNetworkPlayerManager();
        services.AddSingleton<IMessageHandler, CompanionNetworkPlayerManager>(_ => networkPlayerManager);
        services.AddSingleton(networkPlayerManager);
        services.AddSingleton<CompanionMessageHandler>();
        services.AddSingleton<CompanionConnectionManager>();
        services.AddSingleton<PoiLoader>();
        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }

    private static GameData? BuildGameDataInstance()
    {
        Logger.Info("Loading data.json");
        var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "data.json"));
        return JsonConvert.DeserializeObject<GameData>(json);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!string.IsNullOrEmpty(Settings.Default.Theme) && !string.IsNullOrEmpty(Settings.Default.ThemeAccent))
        {
            ThemeManager.Current.ChangeTheme(this, $"{Settings.Default.Theme}.{Settings.Default.ThemeAccent}");
        }

        Logger.Debug("OnStartup()");
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
    }

    private void SetupExceptionHandling()
    {
        Logger.Debug("Setting up exception handling");
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

        DispatcherUnhandledException += (_, e) =>
        {
            LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException", false);
            e.SetObserved();
        };
    }

    private static void LogUnhandledException(Exception exception, string source, bool sendEvent = true)
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
            if (exception is AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.Flatten().InnerExceptions)
                {
                    Logger.Error(innerException);
                }
            }
            else
            {
                Logger.Error(exception, message);
            }
        }

        if (sendEvent)
        {
            WeakReferenceMessenger.Default.Send(new UnhandledExceptionEvent(exception));
        }
    }

    public static void GetAssemblyVersion(out string assemblyName, out SemVersion assemblyVersion)
    {
        var assemblyInfo = Assembly.GetExecutingAssembly()
            .GetName();
        assemblyName = assemblyInfo.Name ?? "SOTFEdit";
        assemblyVersion = SemVersion.FromVersion(assemblyInfo.Version);
    }
}