using System;
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly ArmorPageViewModel _armorPageViewModel;

    [ObservableProperty] private bool _checkVersionOnStartup;

    [ObservableProperty] private string _lastSaveGameMenuItem = "Open last savegame...";

    [ObservableProperty] private double? _pinLeft;

    [ObservableProperty] private Position? _pinPos;

    [ObservableProperty] private double? _pinTop;

    public MainViewModel(ArmorPageViewModel armorPageViewModel, GamePage gamePage, NpcsPage npcsPage,
        StructuresPage structuresPage)
    {
        _armorPageViewModel = armorPageViewModel;
        GamePage = gamePage;
        NpcsPage = npcsPage;
        StructuresPage = structuresPage;
        CheckVersionOnStartup = Settings.Default.CheckForUpdates;

        UpdateLastSaveGameMenuItem();

        SetupListeners();
    }

    public GamePage GamePage { get; }
    public InventoryPage InventoryPage { get; } = new();
    public FollowersPage FollowersPage { get; } = new();
    public PlayerPage PlayerPage { get; } = new();
    public StoragePage StoragePage { get; } = new();
    public NpcsPage NpcsPage { get; }
    public StructuresPage StructuresPage { get; }

    public object? SelectedTab { get; set; }


    partial void OnCheckVersionOnStartupChanged(bool value)
    {
        Settings.Default.CheckForUpdates = value;
        Settings.Default.Save();
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ReloadSavegame()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var replacementSavegame = SavegameManager.ReloadSavegame(selectedSavegame);
        SavegameManager.SelectedSavegame = replacementSavegame;
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
            (_, _) => { ReloadSavegame(); });

        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, message) => { OnSelectedSavegameChanged(message); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        SaveChangesCommand.NotifyCanExecuteChanged();
        ReloadSavegameCommand.NotifyCanExecuteChanged();
        OpenSavegameDirCommand.NotifyCanExecuteChanged();
        DeleteBackupsCommand.NotifyCanExecuteChanged();
        ExperimentResetKillStatisticsCommand.NotifyCanExecuteChanged();
        ExperimentResetNumberCutTreesCommand.NotifyCanExecuteChanged();
        RestoreBackupsCommand.NotifyCanExecuteChanged();
        RegrowTreesCommand.NotifyCanExecuteChanged();
        ModifyConsumedItemsCommand.NotifyCanExecuteChanged();
        IgniteAndRefuelFiresCommand.NotifyCanExecuteChanged();
        ResetStructureDamageCommand.NotifyCanExecuteChanged();
        TeleportWorldItemCommand.NotifyCanExecuteChanged();
        Settings.Default.LastSavegame = message.SelectedSavegame?.FullPath ?? "";
        Settings.Default.Save();
        UpdateLastSaveGameMenuItem();
        OpenLastSavegameCommand.NotifyCanExecuteChanged();
    }

    private void UpdateLastSaveGameMenuItem()
    {
        if (Settings.Default.LastSavegame is not { } lastSavegame)
        {
            LastSaveGameMenuItem = "Open last savegame...";
            return;
        }

        var parts = lastSavegame.Split(Path.DirectorySeparatorChar);
        if (parts.Length < 2)
        {
            LastSaveGameMenuItem = $"Open {lastSavegame}...";
            return;
        }

        LastSaveGameMenuItem = $"Open {parts[^2]}{Path.DirectorySeparatorChar}{parts[^1]}...";
    }

    public static bool CanOpenLastSavegame()
    {
        return !string.IsNullOrEmpty(Settings.Default.LastSavegame) && Directory.Exists(Settings.Default.LastSavegame);
    }

    [RelayCommand(CanExecute = nameof(CanOpenLastSavegame))]
    private void OpenLastSavegame()
    {
        if (!CanOpenLastSavegame())
        {
            WeakReferenceMessenger.Default.Send(
                new GenericMessageEvent("Unable to open the last savegame. Has it been deleted?", "Error"));
        }

        var directoryInfo = new DirectoryInfo(Settings.Default.LastSavegame);
        SavegameManager.SelectedSavegame = SavegameManager.CreateSaveInfo(directoryInfo);
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void SaveChanges()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(selectedSavegame, backupMode =>
        {
            var hasChanges = Ioc.Default.GetRequiredService<GamePage>().Update(selectedSavegame);
            hasChanges = InventoryPage.Update(selectedSavegame) || hasChanges;
            hasChanges = _armorPageViewModel.Update(selectedSavegame) || hasChanges;
            hasChanges = FollowersPage.Update(selectedSavegame) || hasChanges;
            hasChanges = PlayerPage.Update(selectedSavegame) || hasChanges;
            hasChanges = StoragePage.Update(selectedSavegame) || hasChanges;
            hasChanges = StructuresPage.Update(selectedSavegame) || hasChanges;

            hasChanges = selectedSavegame.SavegameStore.SaveAllModified(backupMode,
                ApplicationSettings.BackupFlags) || hasChanges;

            if (hasChanges)
            {
                WeakReferenceMessenger.Default.Send(new SavegameStoredEvent("Changes saved successfully"));
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new GenericMessageEvent("No changes - Nothing saved",
                    "No changes"));
            }
        }));
    }

    public static bool IsSavegameSelected()
    {
        return SavegameManager.SelectedSavegame != null;
    }

    [RelayCommand]
    private static void OpenUrl(string url)
    {
        WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForUrl(url));
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void OpenSavegameDir()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForExplorer(selectedSavegame.FullPath));
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void DeleteBackups()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestDeleteBackupsEvent(selectedSavegame));
    }

    [RelayCommand]
    private static void CheckForUpdates()
    {
        WeakReferenceMessenger.Default.Send(new RequestCheckForUpdatesEvent(true, true));
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ExperimentResetKillStatistics()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        try
        {
            LabExperiments.ResetKillStatistics(selectedSavegame);
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent("Changes added. Please save to persist them",
                "Changes added"));
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent($"An exception has occured: {ex.Message}",
                "Error"));
        }
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ExperimentResetNumberCutTrees()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        try
        {
            LabExperiments.ResetNumberCutTrees(selectedSavegame);
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent("Changes added. Please save to persist them",
                "Changes added"));
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent($"An exception has occured: {ex.Message}",
                "Error"));
        }
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ModifyConsumedItems()
    {
        WeakReferenceMessenger.Default.Send(new RequestModifyConsumedItemsEvent());
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void RestoreBackups(string restoreFromNewest)
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestRestoreBackupsEvent(selectedSavegame,
            bool.Parse(restoreFromNewest)));
    }

    [RelayCommand]
    private static void ChangeSettings()
    {
        WeakReferenceMessenger.Default.Send(new RequestChangeSettingsEvent());
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void RegrowTrees()
    {
        WeakReferenceMessenger.Default.Send(new RequestRegrowTreesEvent());
    }

    public void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F5 && ReloadSavegameCommand.CanExecute(null))
        {
            e.Handled = true;
            ReloadSavegameCommand.Execute(null);
        }

        if (e.Key != Key.S || Keyboard.Modifiers != ModifierKeys.Control || !SaveChangesCommand.CanExecute(null))
        {
            return;
        }

        e.Handled = true;
        SaveChangesCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ResetStructureDamage()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame ||
            selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.StructureDestructionSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.StructureDestruction) is not
                { } structureDestruction ||
            structureDestruction["Data"] is not JArray destructionData)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent("No structure damage has to be repaired",
                "No structure damage"));
            return;
        }

        var entryCount = destructionData.Count;
        destructionData.Clear();
        saveDataWrapper.MarkAsModified(Constants.JsonKeys.StructureDestruction);

        WeakReferenceMessenger.Default.Send(
            new GenericMessageEvent($"{entryCount} structural damages have been repaired", "Damages repaired"));
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void IgniteAndRefuelFires()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame ||
            selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.FiresSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.Fires) is not { } fires ||
            fires["FiresPerStructureType"] is not { } firesPerStructureType)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent("No fires found to be modified", "No fires"));
            return;
        }

        var countChanged = 0;

        foreach (var fireType in firesPerStructureType)
        foreach (var firesByType in fireType.Children())
        foreach (var fire in firesByType)
        {
            fire["IsLit"]?.Replace(true);
            fire["Fuel"]?.Replace(65535.0f);
            fire["FuelDrainRate"]?.Replace(0.00001f);
            countChanged++;
        }

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.Fires);
        WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
            $"{countChanged} fires have been lit, refueled and their drain rate lowered", "Fires changed"));
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void TeleportWorldItem()
    {
        WeakReferenceMessenger.Default.Send(new RequestTeleportWorldItemEvent());
    }
}