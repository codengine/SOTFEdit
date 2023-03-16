using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private bool _backupFiles;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegrowTreesCommand))]
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
    public WeatherPage WeatherPage { get; } = new();
    public GameStatePage GameStatePage { get; } = new();
    public FollowersPage FollowersPage { get; } = new();

    public object? SelectedTab { get; set; }

    [NotifyCanExecuteChangedFor(nameof(RegrowTreesCommand))]
    [NotifyPropertyChangedFor(nameof(VegetationStateIsAllSelected))]
    [ObservableProperty]
    private VegetationState _vegetationStateSelected =
        VegetationState.Gone | VegetationState.HalfChopped | VegetationState.Stumps;

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

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SavegameStoredEvent>(this,
            (_, message) =>
            {
                if (!message.ReloadSavegame)
                {
                    return;
                }

                if (SelectedSavegame != null)
                {
                    var replacementSavegame = SavegameManager.ReloadSavegame(SelectedSavegame);
                    SelectedSavegame = replacementSavegame;
                }
                else
                {
                    SavegameManager.LoadSavegames();
                }
            });
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
            var hasChanges = GameSetupPage.Update(SelectedSavegame, createBackup);
            hasChanges = InventoryPage.Update(SelectedSavegame, createBackup) || hasChanges;
            hasChanges = ArmorPage.Update(SelectedSavegame, createBackup) || hasChanges;
            hasChanges = WeatherPage.Update(SelectedSavegame, createBackup) || hasChanges;
            hasChanges = GameStatePage.Update(SelectedSavegame, createBackup) || hasChanges;
            hasChanges = FollowersPage.Update(SelectedSavegame, createBackup) || hasChanges;

            var message = hasChanges ? "Changes saved successfully" : "No changes - Nothing saved";

            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent(message, hasChanges));
        }));
    }

    public bool CanSaveChanges()
    {
        return SelectedSavegame != null;
    }

    partial void OnSelectedSavegameChanged(Savegame? value)
    {
        WeakReferenceMessenger.Default.Send(new SelectedSavegameChangedEvent(value));
    }

    private bool CanRegrowTrees()
    {
        return CanSaveChanges() && VegetationStateSelected != VegetationState.None;
    }

    [RelayCommand(CanExecute = nameof(CanRegrowTrees))]
    public void RegrowTrees()
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestRegrowTreesEvent(SelectedSavegame, BackupFiles,
            VegetationStateSelected));
    }
}