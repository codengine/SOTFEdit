using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using Semver;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
// ReSharper disable once UnusedMember.Global
public partial class MainWindow
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly string _baseTitle;

    public MainWindow()
    {
        SetupListeners();
        DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
        InitializeComponent();

        GetAssemblyVersion(out var assemblyName, out var assemblyVersion);

        _baseTitle = $"{assemblyName} v{assemblyVersion}";
        Title = _baseTitle;
    }

    private static void GetAssemblyVersion(out string assemblyName, out SemVersion assemblyVersion)
    {
        var assemblyInfo = Assembly.GetExecutingAssembly()
            .GetName();
        assemblyName = assemblyInfo.Name ?? "SOTFEdit";
        assemblyVersion = SemVersion.FromVersion(assemblyInfo.Version);
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SavegameStoredEvent>(this,
            (_, message) => { OnSavegameStored(message); });
        WeakReferenceMessenger.Default.Register<RequestRegrowTreesEvent>(this,
            (_, message) => { OnRequestRegrowTreesEvent(message); });
        WeakReferenceMessenger.Default.Register<RequestReviveFollowersEvent>(this,
            (_, message) => { OnRequestReviveFollowersEvent(message); });
        WeakReferenceMessenger.Default.Register<RequestSaveChangesEvent>(this,
            (_, message) => { OnRequestSaveChangesEvent(message); });
        WeakReferenceMessenger.Default.Register<RequestSelectSavegameEvent>(this,
            (_, _) => { OnRequestSelectSavegameEvent(); });
        WeakReferenceMessenger.Default.Register<RequestApplicationExitEvent>(this,
            (_, _) => { OnRequestApplicationExitEvent(); });
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, message) => { OnSelectedSavegameChangedEvent(message.SelectedSavegame); });
        WeakReferenceMessenger.Default.Register<RequestStartProcessEvent>(this,
            (_, message) => { OnRequestStartProcessEvent(message); });
        WeakReferenceMessenger.Default.Register<RequestDeleteBackupsEvent>(this,
            (_, message) => { OnRequestDeleteBackupsEvent(message); });
        WeakReferenceMessenger.Default.Register<RequestCheckForUpdatesEvent>(this,
            (_, message) => { OnRequestCheckForUpdatesEvent(message); });
        WeakReferenceMessenger.Default.Register<VersionCheckResultEvent>(this,
            (_, message) => { OnVersionCheckResultEvent(message); });
    }

    private void OnVersionCheckResultEvent(VersionCheckResultEvent message)
    {
        if (message.IsError)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(this, "An error occured while checking for latest version", "Error"));
            return;
        }

        if (!message.IsNewer)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(this, "You are already using the latest version", "No update"));
            return;
        }

        var result =
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(this,
                $"A new version is available: {message.LatestTagVersion}\nDo you want to go to the release page?",
                "New Version available", MessageBoxButton.YesNo));
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var link = message.Link ?? Ioc.Default.GetRequiredService<GameData>().Config.LatestReleaseUrl;

        Process.Start(new ProcessStartInfo()
        {
            FileName = link,
            UseShellExecute = true
        });
    }

    protected override void OnContentRendered(EventArgs e)
    {
        if (Settings.Default.CheckForUpdates)
        {
            CheckForUpdate(false, false);
        }
    }

    private static void OnRequestCheckForUpdatesEvent(RequestCheckForUpdatesEvent message)
    {
        CheckForUpdate(message.NotifyOnSameVersion, message.NotifyOnError);
    }

    private static void CheckForUpdate(bool notifyOnSameVersion, bool notifyOnError)
    {
        Ioc.Default.GetRequiredService<UpdateChecker>()
            .CheckForUpdates(notifyOnSameVersion, notifyOnError);
    }

    private void OnRequestDeleteBackupsEvent(RequestDeleteBackupsEvent message)
    {
        var result = MessageBox.Show(this, "Do you really want to delete all backups?", "Delete all backups",
            MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var countDeleted = message.Savegame.SavegameStore.DeleteBackups();
            MessageBox.Show(this, $"Deleted {countDeleted} backups", "Success");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Unable to delete backups at {message.Savegame.FullPath}");
            MessageBox.Show(this, $"Unable to delete backups: {ex.Message}");
        }
    }

    private static void OnRequestStartProcessEvent(RequestStartProcessEvent message)
    {
        Process.Start(message.ProcessStartInfo);
    }

    private void OnSelectedSavegameChangedEvent(Savegame? selectedSavegame)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Title = _baseTitle + (selectedSavegame != null
                ? $" (Selected: {selectedSavegame.Title}, {selectedSavegame.PrintableType} - Saved at: {selectedSavegame.LastSaveTime})"
                : "");
        });
    }

    private static void OnRequestApplicationExitEvent()
    {
        Application.Current.Shutdown();
    }

    private void OnRequestSelectSavegameEvent()
    {
        var window = new SelectSavegameWindow
        {
            Owner = this
        };
        window.ShowDialog();
    }

    private void OnRequestSaveChangesEvent(RequestSaveChangesEvent message)
    {
        if (message.SelectedSavegame.SavegameStore.HasChanged())
        {
            var overwriteResult =
                MessageBox.Show(this,
                    "The savegame has been modified outside. Do you really want to overwrite any changes?",
                    "Overwrite Changes", MessageBoxButton.YesNo);
            if (overwriteResult != MessageBoxResult.Yes)
            {
                return;
            }
        }

        message.InvokeCallback();
    }

    private static void OnRequestRegrowTreesEvent(RequestRegrowTreesEvent message)
    {
        WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(message.SelectedSavegame, message.BackupFiles,
            createBackup =>
            {
                var countRegrown = message.SelectedSavegame.RegrowTrees(createBackup, message.VegetationStateSelected);
                var resultMessage =
                    countRegrown == 0
                        ? $"No trees to regrow found with state \"{message.VegetationStateSelected}\""
                        : $"{countRegrown} trees with previous state \"{message.VegetationStateSelected}\" should now have regrown";
                WeakReferenceMessenger.Default.Send(new SavegameStoredEvent(resultMessage, countRegrown > 0));
            }));
    }

    private static void OnRequestReviveFollowersEvent(RequestReviveFollowersEvent message)
    {
        WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(message.SelectedSavegame, message.BackupFiles,
            createBackup =>
            {
                var hasChanges = message.SelectedSavegame.ReviveFollower(message.TypeId, createBackup);

                var actorName = message.TypeId == Constants.Actors.KelvinTypeId ? "Kelvin" : "Virginia";

                var resultMessage = hasChanges
                    ? $"{actorName} should now be back again"
                    : $"{actorName} should be alive already";

                WeakReferenceMessenger.Default.Send(new SavegameStoredEvent(resultMessage, hasChanges));
            }));
    }

    private void OnSavegameStored(SavegameStoredEvent message)
    {
        MessageBox.Show(this, message.Message);
    }
}