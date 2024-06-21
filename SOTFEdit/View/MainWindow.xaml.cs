using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Infrastructure.Converters;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;
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

    private readonly MainViewModel _dataContext;

    private Window? _exceptionWindowOwner;
    private MapWindow? _mapWindow;
    private int? _selectedPosSenderHash;

    public MainWindow()
    {
        SetupListeners();
        DataContext = _dataContext = Ioc.Default.GetRequiredService<MainViewModel>();
        InitializeComponent();

        App.GetAssemblyVersion(out var assemblyName, out var assemblyVersion);

        _baseTitle = $"{assemblyName} v{assemblyVersion}";
        Title = _baseTitle;

        Loaded += OnLoaded;
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SavegameStoredEvent>(this,
            (_, message) => OnSavegameStored(message));
        WeakReferenceMessenger.Default.Register<RequestRegrowTreesEvent>(this,
            (_, _) => OnRequestRegrowTreesEvent());
        WeakReferenceMessenger.Default.Register<RequestChangeUnlocksEvent>(this,
            (_, _) => OnRequestChangeUnlocksEvent());
        WeakReferenceMessenger.Default.Register<RequestItemPlaterEvent>(this,
            (_, _) => OnRequestItemPlaterEvent());
        WeakReferenceMessenger.Default.Register<RequestReviveFollowersEvent>(this,
            (_, message) => OnRequestReviveFollowersEvent(message));
        WeakReferenceMessenger.Default.Register<RequestSaveChangesEvent>(this,
            (_, message) => OnRequestSaveChangesEvent(message));
        WeakReferenceMessenger.Default.Register<RequestSelectSavegameEvent>(this,
            (_, _) => OnRequestSelectSavegameEvent());
        WeakReferenceMessenger.Default.Register<RequestApplicationExitEvent>(this,
            (_, _) => OnRequestApplicationExitEvent());
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, message) => OnSelectedSavegameChangedEvent(message.SelectedSavegame));
        WeakReferenceMessenger.Default.Register<RequestStartProcessEvent>(this,
            (_, message) => OnRequestStartProcessEvent(message));
        WeakReferenceMessenger.Default.Register<RequestDeleteBackupsEvent>(this,
            (_, message) => OnRequestDeleteBackupsEvent(message));
        WeakReferenceMessenger.Default.Register<RequestCheckForUpdatesEvent>(this,
            (_, message) => OnRequestCheckForUpdatesEvent(message));
        WeakReferenceMessenger.Default.Register<VersionCheckResultEvent>(this,
            (_, message) => OnVersionCheckResultEvent(message));
        WeakReferenceMessenger.Default.Register<RequestSpawnFollowerEvent>(this,
            (_, message) => OnRequestSpawnFollowerEvent(message));
        WeakReferenceMessenger.Default.Register<RequestRestoreBackupsEvent>(this,
            (_, message) => OnRequestRestoreBackupsEvent(message));
        WeakReferenceMessenger.Default.Register<UnhandledExceptionEvent>(this,
            (_, message) => OnUnhandledExceptionEvent(message));

        WeakReferenceMessenger.Default.Register<ZoomToPosEvent>(this, (_, message) => OnZoomToPos(message));
        WeakReferenceMessenger.Default.Register<RequestChangeSettingsEvent>(this,
            (_, _) => OnRequestChangeSettingsEvent());
        WeakReferenceMessenger.Default.Register<RequestEditActorEvent>(this,
            (_, message) => OnRequestEditActorEvent(message));
        WeakReferenceMessenger.Default.Register<GenericMessageEvent>(this,
            (_, message) => OnGenericMessageEvent(message));
        WeakReferenceMessenger.Default.Register<RequestModifyConsumedItemsEvent>(this,
            (_, _) => OnRequestModifyConsumedItemsEvent());
        WeakReferenceMessenger.Default.Register<RequestTeleportWorldItemEvent>(this,
            (_, _) => OnRequestTeleportWorldItemEvent());
        WeakReferenceMessenger.Default.Register<ShowDialogEvent>(this,
            (_, message) => OnShowDialogEvent(message));
        WeakReferenceMessenger.Default.Register<RequestOpenMapEvent>(this,
            (_, message) => OnRequestOpenMapEvent(message));
        WeakReferenceMessenger.Default.Register<UpdateActorsEvent>(this,
            (_, message) => OnUpdateActorsEvent(message));
        WeakReferenceMessenger.Default.Register<OpenCompanionSetupWindowEvent>(this,
            (_, _) => OnOpenCompanionSetupWindowEvent());
    }

    private void OnOpenCompanionSetupWindowEvent()
    {
        var window = new CompanionSetupWindow(this);
        window.ShowDialog();
    }

    private void OnRequestOpenMapEvent(RequestOpenMapEvent message)
    {
        if (_mapWindow == null) // Check if the window is not already open
        {
            _mapWindow = new MapWindow(message)
            {
                MapZoomControl =
                {
                    AnimationLength = TimeSpan.FromMilliseconds(200)
                }
            };
            _mapWindow.Closed += MapWindowClosed; // Subscribe to the Closed event of the window
            _dataContext.IsMapWindowClosed = false;
            _mapWindow.Show();
        }
        else
        {
            _mapWindow.Focus(); // Bring the existing window to the foreground
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _mapWindow?.Close();
    }

    private void MapWindowClosed(object? sender, EventArgs e)
    {
        if (_mapWindow != null)
        {
            _mapWindow.Closed -= MapWindowClosed;
        }

        _mapWindow = null; // Reset the reference to null when the window is closed
        _dataContext.IsMapWindowClosed = true;
    }

    private async void OnShowDialogEvent(ShowDialogEvent message)
    {
        var dialog = message.DialogFactory.Invoke(this);
        await this.ShowMetroDialogAsync(dialog, new MetroDialogSettings
        {
            AnimateShow = false,
            AnimateHide = false,
            OwnerCanCloseWithDialog = true
        });
        await dialog.WaitUntilUnloadedAsync();
    }

    private static void OnUpdateActorsEvent(UpdateActorsEvent message)
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        Ioc.Default.GetRequiredService<ActorModifier>().Modify(selectedSavegame, message);
        WeakReferenceMessenger.Default.Send(new JsonModelChangedEvent(SavegameStore.FileType.SaveData));
    }

    private void OnRequestTeleportWorldItemEvent()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var window = new WorldItemTeleportWindow(this, selectedSavegame);
        window.Show();
    }

    private void OnRequestModifyConsumedItemsEvent()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var window = new ModifyConsumedItemsWindow(this, selectedSavegame, Ioc.Default.GetRequiredService<GameData>());
        window.ShowDialog();
    }

    private async void OnGenericMessageEvent(GenericMessageEvent message)
    {
        await ShowMessageDialog(message.Message, message.Title);
    }

    private void OnRequestEditActorEvent(RequestEditActorEvent message)
    {
        var window = new EditActorWindow(message.Owner ?? this, message.Actor);
        window.ShowDialog();
    }

    private void OnRequestChangeSettingsEvent()
    {
        var applicationSettings = Ioc.Default.GetRequiredService<ApplicationSettings>();
        var dialog = new SettingsDialog(this, applicationSettings);
        dialog.ShowDialog();
    }

    private async void OnRequestChangeUnlocksEvent()
    {
        var savePath = SavegameManager.GetSavegamePathFromAppData();
        if (string.IsNullOrEmpty(savePath) || !Directory.Exists(savePath) ||
            GetPathToPlayerProfile(savePath) is not { } playerProfilePath)
        {
            await ShowMessageDialog(
                string.Format(TranslationManager.Get("windows.unlocks.messages.playerProfileNotFound"), savePath),
                TranslationManager.Get("generic.error"));
            return;
        }

        var dialog = new UnlocksWindow(this, playerProfilePath);
        dialog.ShowDialog();
    }

    private void OnRequestItemPlaterEvent()
    {
        var dialog = new ItemPlaterWindow(this, Ioc.Default.GetRequiredService<GameData>());
        dialog.ShowDialog();
    }

    private static string? GetPathToPlayerProfile(string savePath)
    {
        string? fallback = null;
        foreach (var directoryInfo in new DirectoryInfo(savePath).GetDirectories())
        {
            var path = Path.Combine(directoryInfo.FullName, "PlayerProfile.json");
            if (File.Exists(path))
            {
                return path;
            }

            if (fallback == null && directoryInfo.Name.All(char.IsDigit))
            {
                fallback = path;
            }
        }

        return fallback;
    }

    private void OnZoomToPos(ZoomToPosEvent message)
    {
        var senderHash = RuntimeHelpers.GetHashCode(message.Pos);

        if (senderHash == _selectedPosSenderHash)
        {
            MapFlyout.IsOpen = false;
            _selectedPosSenderHash = null;
            return;
        }

        var ingameToPixel = CoordinateConverter.IngameToPixel(message.Pos.X, message.Pos.Z);
        ZoomCtrl.Zoom = 2;
        ZoomCtrl.TranslateX = -2 * ingameToPixel.Item1 + ZoomCtrl.ActualWidth;
        ZoomCtrl.TranslateY = -2 * ingameToPixel.Item2 + ZoomCtrl.ActualHeight;
        _dataContext.PinLeft = ingameToPixel.Item1 - 16;
        _dataContext.PinTop = ingameToPixel.Item2 - 16;
        _dataContext.PinPos = message.Pos;

        _selectedPosSenderHash = senderHash;
        MapFlyout.IsOpen = true;
    }

    private void OnUnhandledExceptionEvent(UnhandledExceptionEvent message)
    {
        var action = new Action(() =>
        {
            var unhandledExceptionWindow = new UnhandledExceptionWindow(_exceptionWindowOwner, message.Exception);
            unhandledExceptionWindow.ShowDialog();
        });

        if (Application.Current.Dispatcher.CheckAccess())
        {
            action.Invoke();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(action);
        }
    }

    private async void OnRequestRestoreBackupsEvent(RequestRestoreBackupsEvent message)
    {
        var result = await ShowConfirmDialog(TranslationManager.Get("windows.main.messages.confirmRestoreBackups.text"),
            TranslationManager.Get("windows.main.messages.confirmRestoreBackups.title"));
        if (result != MessageDialogResult.Affirmative)
        {
            return;
        }

        var countRestored =
            message.Savegame.SavegameStore.RestoreBackups(message.RestoreFromNewest);

        if (countRestored > 0)
        {
            _dataContext.ReloadSavegameCommand.Execute(null);
        }

        await ShowMessageDialog(
            TranslationManager.GetFormatted("backup.messages.backupsRestored.text", countRestored),
            TranslationManager.Get("backup.messages.backupsRestored.title"));
    }

    private async void OnRequestSpawnFollowerEvent(RequestSpawnFollowerEvent message)
    {
        var dialog = new SpawnFollowerInputDialog(this)
        {
            Max = message.TypeId switch
            {
                Constants.Actors.KelvinTypeId => 20,
                Constants.Actors.VirginiaTypeId => 4,
                _ => 1
            }
        };

        if (dialog.ShowDialog() is not true || dialog.Count is not { } count || count < 1)
        {
            return;
        }

        if (message.Savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not { } saveDataWrapper)
        {
            Logger.Warn("Save data could not be loaded");
            await ShowMessageDialog(TranslationManager.Get("windows.main.messages.unableToSpawnFollowers"),
                TranslationManager.Get("generic.error"));
            return;
        }

        var followerModifier = new FollowerModifier(saveDataWrapper);
        var hasChanges =
            followerModifier.CreateFollowers(message.TypeId, count, message.ItemIds, message.Outfit, message.Pos);

        if (hasChanges)
        {
            await ShowMessageDialog(
                TranslationManager.GetFormatted("windows.main.messages.followersCreated.text", count),
                TranslationManager.Get("windows.main.messages.followersCreated.title"));
        }
    }

    private async void OnVersionCheckResultEvent(VersionCheckResultEvent message)
    {
        if (!message.InvokedManually && message.LatestTagVersion?.ToString() is { } latestTagVersionAsString)
        {
            if (!message.InvokedManually && latestTagVersionAsString.Equals(Settings.Default.LastFoundVersion))
            {
                return;
            }

            Settings.Default.LastFoundVersion = latestTagVersionAsString;
            Settings.Default.Save();
        }

        if (message.IsError)
        {
            await Application.Current.Dispatcher.Invoke(async () =>
                await ShowMessageDialog(TranslationManager.Get("windows.main.errors.errorCheckingLastVersion"),
                    TranslationManager.Get("generic.error")));
            return;
        }

        if (!message.IsNewer)
        {
            await Application.Current.Dispatcher.Invoke(async () =>
                await ShowMessageDialog(TranslationManager.Get("windows.main.messages.alreadyLatestVersion.text"),
                    TranslationManager.Get("windows.main.messages.alreadyLatestVersion.title")));
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            var window = new UpdateAvailableWindow(this, message);
            window.ShowDialog();
        });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _exceptionWindowOwner = this;
        if (Settings.Default.CheckForUpdates)
        {
            CheckForUpdate(false, false, false);
        }
    }

    private static void OnRequestCheckForUpdatesEvent(RequestCheckForUpdatesEvent message)
    {
        CheckForUpdate(message.NotifyOnSameVersion, message.NotifyOnError, true);
    }

    private static void CheckForUpdate(bool notifyOnSameVersion, bool notifyOnError, bool invokedManually)
    {
        Ioc.Default.GetRequiredService<UpdateChecker>()
            .CheckForUpdates(notifyOnSameVersion, notifyOnError, invokedManually);
    }

    private async void OnRequestDeleteBackupsEvent(RequestDeleteBackupsEvent message)
    {
        var result = await ShowConfirmDialog(TranslationManager.Get("windows.main.messages.confirmDeleteBackups.text"),
            TranslationManager.Get("windows.main.messages.confirmDeleteBackups.title"));
        if (result != MessageDialogResult.Affirmative)
        {
            return;
        }

        try
        {
            var countDeleted = message.Savegame.SavegameStore.DeleteBackups();
            await ShowMessageDialog(
                TranslationManager.GetFormatted("windows.main.messages.backupsDeleted", countDeleted),
                TranslationManager.Get("generic.success"));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Unable to delete backups at {message.Savegame.FullPath}");
            await ShowMessageDialog(
                TranslationManager.GetFormatted("windows.main.messages.unableToDeleteBackups", ex.Message),
                TranslationManager.Get("generic.error"));
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
                ? TranslationManager.GetFormatted("windows.main.title", selectedSavegame.Title,
                    selectedSavegame.PrintableType, selectedSavegame.LastSaveTime,
                    selectedSavegame.SavegameStore.LastWriteTime)
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

    private async void OnRequestSaveChangesEvent(RequestSaveChangesEvent message)
    {
        if (message.SelectedSavegame.SavegameStore.HasChanged())
        {
            var overwriteResult =
                await ShowConfirmDialog(
                    TranslationManager.Get("windows.main.messages.confirmOverwriteExternalChanges.text"),
                    TranslationManager.Get("windows.main.messages.confirmOverwriteExternalChanges.title"));
            if (overwriteResult != MessageDialogResult.Affirmative)
            {
                return;
            }
        }

        var applicationSettings = Ioc.Default.GetRequiredService<ApplicationSettings>();
        var effectiveBackupMode = applicationSettings.CurrentBackupMode;

        if (Settings.Default.AskForBackups && effectiveBackupMode != ApplicationSettings.BackupMode.None)
        {
            var dialogSettings = BuildDefaultDialogSettings();
            dialogSettings.AffirmativeButtonText = TranslationManager.Get("generic.yes");
            dialogSettings.NegativeButtonText = TranslationManager.Get("generic.no");
            dialogSettings.FirstAuxiliaryButtonText = TranslationManager.Get("generic.cancel");
            dialogSettings.DialogResultOnCancel = MessageDialogResult.Canceled;

            var askResult = await ShowConfirmDialog(
                TranslationManager.Get("windows.main.messages.confirmCreateBackup.text"),
                TranslationManager.Get("windows.main.messages.confirmCreateBackup.title"),
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, dialogSettings);

            switch (askResult)
            {
                case MessageDialogResult.Negative:
                    effectiveBackupMode = ApplicationSettings.BackupMode.None;
                    break;
                case MessageDialogResult.Canceled:
                case MessageDialogResult.FirstAuxiliary:
                    return;
            }
        }

        message.InvokeCallback(effectiveBackupMode);
    }

    private void OnRequestRegrowTreesEvent()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var window = new RegrowTreesWindow(this, selectedSavegame);
        window.ShowDialog();
    }

    private static void OnRequestReviveFollowersEvent(RequestReviveFollowersEvent message)
    {
        var hasChanges =
            message.SelectedSavegame.ReviveFollower(message.TypeId, message.ItemIds, message.Outfit, message.Pos);

        var actorName = TranslationManager.Get("actors.types." + message.TypeId);

        if (hasChanges)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.GetFormatted("windows.main.messages.followerRevived.text", actorName),
                TranslationManager.Get("windows.main.messages.followerRevived.title")));
            WeakReferenceMessenger.Default.Send(new JsonModelChangedEvent(SavegameStore.FileType.SaveData));
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.GetFormatted("windows.main.messages.followerAlreadyAlive.text", actorName),
                TranslationManager.Get("windows.main.messages.followerAlreadyAlive.title")));
        }
    }

    private async void OnSavegameStored(SavegameStoredEvent message)
    {
        if (message.Message is { } text)
        {
            await ShowMessageDialog(text, TranslationManager.Get("windows.main.messages.saveChanges"));
        }
    }

    private Task<MessageDialogResult> ShowMessageDialog(string message, string title)
    {
        var func = () => this.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, new MetroDialogSettings
        {
            AnimateShow = false,
            AnimateHide = false,
            ColorScheme = MetroDialogColorScheme.Theme
        });

        return Application.Current.Dispatcher.CheckAccess()
            ? func.Invoke()
            : Application.Current.Dispatcher.Invoke(func);
    }

    private Task<MessageDialogResult> ShowConfirmDialog(string message, string title)
    {
        return ShowConfirmDialog(message, title, MessageDialogStyle.AffirmativeAndNegative,
            BuildDefaultDialogSettings());
    }

    private static MetroDialogSettings BuildDefaultDialogSettings()
    {
        return new MetroDialogSettings
        {
            AnimateShow = false,
            AnimateHide = false,
            ColorScheme = MetroDialogColorScheme.Theme
        };
    }

    private Task<MessageDialogResult> ShowConfirmDialog(string message, string title, MessageDialogStyle dialogStyle,
        MetroDialogSettings dialogSettings)
    {
        return this.ShowMessageAsync(title, message, dialogStyle, dialogSettings);
    }

    private void OpenReadme_Click(object sender, RoutedEventArgs e)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "README.md");
        var markdownViewer = new MarkdownViewer(this, path, TranslationManager.Get("menu.help.readme"));
        markdownViewer.ShowDialog();
    }

    private void OpenChangelog_Click(object sender, RoutedEventArgs e)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CHANGELOG.md");
        var markdownViewer = new MarkdownViewer(this, path, TranslationManager.Get("menu.help.changelog"));
        markdownViewer.ShowDialog();
    }

    private async void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Q when Keyboard.Modifiers == ModifierKeys.Control:
            {
                e.Handled = true;

                if (await ShowConfirmDialog(
                        TranslationManager.Get("windows.main.messages.confirmCloseApplication.text"),
                        TranslationManager.Get("windows.main.messages.confirmCloseApplication.title")) ==
                    MessageDialogResult.Affirmative)
                {
                    Close();
                }

                break;
            }
            case Key.Escape:
            {
                if (MapFlyout.IsOpen)
                {
                    MapFlyout.IsOpen = false;
                    e.Handled = true;
                }

                break;
            }
        }

        _dataContext.OnPreviewKeyDown(e);
    }

    private void OpenAbout_OnClick(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow(this);
        aboutWindow.ShowDialog();
    }

    private void OpenTranslations_Click(object sender, RoutedEventArgs e)
    {
        var translationWindow = new TranslationWindow();
        translationWindow.ShowDialog();
    }

    private void MainWindow_OnDragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Move : DragDropEffects.None;
    }

    private void MainWindow_OnDrop(object sender, DragEventArgs e)
    {
        // Handle the drop operation
        if (!e.Data.GetDataPresent(DataFormats.FileDrop) || (string[]?)e.Data.GetData(DataFormats.FileDrop) is not
            {
                Length: 1
            } files)
        {
            return;
        }

        var path = files[0];
        if (Directory.Exists(path))
        {
            SavegameManager.SelectedSavegame = SavegameManager.CreateSaveInfo(new DirectoryInfo(path));
        }
    }
}