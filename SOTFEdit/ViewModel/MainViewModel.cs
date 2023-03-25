using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly ArmorPageViewModel _armorPageViewModel;
    [ObservableProperty] private bool _backupFiles;

    [ObservableProperty] private bool _checkVersionOnStartup;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegrowTreesCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReloadSavegameCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenSavegameDirCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteBackupsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExperimentEnemiesFearThePlayerCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExperimentEnemiesNoFearNoRemorceCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExperimentRemoveAllActorsAndSpawnsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExperimentResetKillStatisticsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExperimentResetNumberCutTreesCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreBackupsCommand))]
    private Savegame? _selectedSavegame;

    [NotifyCanExecuteChangedFor(nameof(RegrowTreesCommand))] [NotifyPropertyChangedFor(nameof(VegetationStateIsAllSelected))] [ObservableProperty]
    private VegetationState _vegetationStateSelected =
        VegetationState.Gone | VegetationState.HalfChopped | VegetationState.Stumps;

    public MainViewModel(SavegameManager savegameManager, ArmorPageViewModel armorPageViewModel)
    {
        _armorPageViewModel = armorPageViewModel;
        SavegameManager = savegameManager;
        BackupFiles = Settings.Default.BackupFiles;
        CheckVersionOnStartup = Settings.Default.CheckForUpdates;
        SetupListeners();
    }

    private SavegameManager SavegameManager { get; }
    public GameSetupPage GameSetupPage { get; } = new();
    public InventoryPage InventoryPage { get; } = new();
    public WeatherPage WeatherPage { get; } = new();
    public GameStatePage GameStatePage { get; } = new();
    public FollowersPage FollowersPage { get; } = new();
    public PlayerPage PlayerPage { get; } = new();
    public StoragePage StoragePage { get; } = new();

    public object? SelectedTab { get; set; }

    public bool VegetationStateIsAllSelected
    {
        get => VegetationStateSelected.HasFlag(VegetationState.Gone) &&
               VegetationStateSelected.HasFlag(VegetationState.HalfChopped) &&
               VegetationStateSelected.HasFlag(VegetationState.Stumps);
        set
        {
            var vegetationState = value == false
                ? VegetationState.None
                : VegetationState.Gone | VegetationState.HalfChopped | VegetationState.Stumps;
            VegetationStateSelected = vegetationState;
        }
    }

    partial void OnCheckVersionOnStartupChanged(bool value)
    {
        Settings.Default.CheckForUpdates = value;
        Settings.Default.Save();
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ReloadSavegame()
    {
        if (SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var replacementSavegame = SavegameManager.ReloadSavegame(selectedSavegame);
        WeakReferenceMessenger.Default.Send(new SelectedSavegameChangedEvent(replacementSavegame));
    }

    [RelayCommand]
    private static void SelectSavegame()
    {
        WeakReferenceMessenger.Default.Send(new RequestSelectSavegameEvent());
    }

    [RelayCommand]
    private static void ExitApplication()
    {
        WeakReferenceMessenger.Default.Send(new RequestApplicationExitEvent());
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SavegameStoredEvent>(this,
            (_, message) =>
            {
                if (!message.ReloadSavegame)
                {
                    return;
                }

                ReloadSavegame();
            });

        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, message) => { SelectedSavegame = message.SelectedSavegame; });
    }

    partial void OnBackupFilesChanged(bool value)
    {
        Settings.Default.BackupFiles = value;
        Settings.Default.Save();
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void SaveChanges()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(SelectedSavegame, BackupFiles, createBackup =>
        {
            var hasChanges = GameSetupPage.Update(SelectedSavegame, createBackup);
            hasChanges = InventoryPage.Update(SelectedSavegame, createBackup) || hasChanges;
            hasChanges = _armorPageViewModel.Update(SelectedSavegame, createBackup) || hasChanges;
            hasChanges = WeatherPage.Update(SelectedSavegame, createBackup) || hasChanges;
            hasChanges = GameStatePage.Update(SelectedSavegame, createBackup) || hasChanges;
            hasChanges = FollowersPage.Update(SelectedSavegame, createBackup) || hasChanges;
            hasChanges = PlayerPage.Update(SelectedSavegame, createBackup) || hasChanges;
            hasChanges = StoragePage.Update(SelectedSavegame, createBackup) || hasChanges;

            var message = hasChanges ? "Changes saved successfully" : "No changes - Nothing saved";

            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent(message, hasChanges));
        }));
    }

    public bool IsSavegameSelected()
    {
        return SelectedSavegame != null;
    }

    private bool CanRegrowTrees()
    {
        return IsSavegameSelected() && VegetationStateSelected != VegetationState.None;
    }

    [RelayCommand]
    private static void OpenUrl(string url)
    {
        WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForUrl(url));
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void OpenSavegameDir()
    {
        if (_selectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForExplorer(selectedSavegame.FullPath));
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void DeleteBackups()
    {
        if (_selectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestDeleteBackupsEvent(selectedSavegame));
    }

    [RelayCommand(CanExecute = nameof(CanRegrowTrees))]
    private void RegrowTrees()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestRegrowTreesEvent(SelectedSavegame, BackupFiles,
            VegetationStateSelected));
    }

    [RelayCommand]
    private static void CheckForUpdates()
    {
        WeakReferenceMessenger.Default.Send(new RequestCheckForUpdatesEvent(true, true));
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ExperimentResetKillStatistics()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        try
        {
            LabExperiments.ResetKillStatistics(SelectedSavegame, BackupFiles);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent("Changes saved"));
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent($"An exception has occured: {ex.Message}",
                false));
        }
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ExperimentResetNumberCutTrees()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        try
        {
            LabExperiments.ResetNumberCutTrees(SelectedSavegame, BackupFiles);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent("Changes saved"));
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent($"An exception has occured: {ex.Message}",
                false));
        }
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ExperimentEnemiesFearThePlayer()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        try
        {
            LabExperiments.EnemiesFearThePlayer(SelectedSavegame, BackupFiles);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent("Changes saved"));
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent($"An exception has occured: {ex.Message}",
                false));
        }
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ExperimentEnemiesNoFearNoRemorce()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        try
        {
            LabExperiments.EnemiesNoFearNoRemorce(SelectedSavegame, BackupFiles);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent("Changes saved"));
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent($"An exception has occured: {ex.Message}",
                false));
        }
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ExperimentRemoveAllActorsAndSpawns()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        try
        {
            LabExperiments.ExperimentRemoveAllActorsAndSpawns(SelectedSavegame, BackupFiles);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent("Changes saved"));
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent($"An exception has occured: {ex.Message}",
                false));
        }
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void RestoreBackups(string restoreFromNewest)
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestRestoreBackupsEvent(SelectedSavegame, bool.Parse(restoreFromNewest)));
    }
}