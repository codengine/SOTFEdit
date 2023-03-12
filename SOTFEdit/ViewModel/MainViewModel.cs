using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private bool _backupFiles;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegrowTreesCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReviveFollowersCommand))]
    private Savegame? _selectedSavegame;

    public MainViewModel(SavegameManager savegameManager)
    {
        SavegameManager = savegameManager;
        BackupFiles = Settings.Default.BackupFiles;
        SetupListeners();
    }

    public SavegameManager SavegameManager { get; }
    public GameSetupPage GameSetupPage { get; } = new();
    public InventoryPage InventoryPage { get; } = new();
    public ArmorPage ArmorPage { get; } = new();

    public object? SelectedTab { get; set; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SavegameStoredEvent>(this,
            (_, _) => { SavegameManager.LoadSavegames(); });
    }

    partial void OnBackupFilesChanged(bool value)
    {
        Settings.Default.BackupFiles = value;
        Settings.Default.Save();
    }

    [RelayCommand]
    public void ReloadSavegames()
    {
        SelectedSavegame = null;
        SavegameManager.LoadSavegames();
    }

    [RelayCommand]
    public void SelectSavegameDir()
    {
        WeakReferenceMessenger.Default.Send(new SelectSavegameDirEvent());
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    public void SaveChanges()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(SelectedSavegame, BackupFiles, createBackup =>
        {
            GameSetupPage.Update(SelectedSavegame, createBackup);
            InventoryPage.Update(SelectedSavegame, createBackup);
            ArmorPage.Update(SelectedSavegame, createBackup);
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent("Changes saved successfully"));
        }));
    }

    public bool CanSaveChanges()
    {
        return SelectedSavegame != null;
    }

    partial void OnSelectedSavegameChanged(Savegame? value)
    {
        WeakReferenceMessenger.Default.Send(new SelectedSavegameChanged(value));
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    public void RegrowTrees()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestRegrowTreesEvent(SelectedSavegame, BackupFiles));
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    public void ReviveFollowers()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestReviveFollowersEvent(SelectedSavegame, BackupFiles));
    }
}

public class SelectedSavegameChanged
{
    public SelectedSavegameChanged(Savegame? selectedSavegame)
    {
        SelectedSavegame = selectedSavegame;
    }

    public Savegame? SelectedSavegame { get; }
}