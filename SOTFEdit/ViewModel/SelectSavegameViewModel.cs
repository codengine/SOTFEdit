using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;

namespace SOTFEdit.ViewModel;

public partial class SelectSavegameViewModel : ObservableObject
{
    private readonly List<Savegame> _savegames;

    [ObservableProperty] private string? _saveDir = SavegameManager.GetSavePath();

    public SelectSavegameViewModel()
    {
        _savegames = new List<Savegame>(SavegameManager.GetSavegames().Values);

        SetupListeners();
    }

    public List<Savegame> SinglePlayerSaves => _savegames
        .Where(savegame => savegame.IsSinglePlayer() || savegame.HasUnknownParentDir())
        .ToList();

    public List<Savegame> MultiPlayerSaves => _savegames
        .Where(savegame => savegame.IsMultiPlayer())
        .ToList();

    public List<Savegame> MultiPlayerClientSaves => _savegames
        .Where(savegame => savegame.IsMultiPlayerClient())
        .ToList();

    [RelayCommand]
    private void SelectSavegame(Savegame savegame)
    {
        WeakReferenceMessenger.Default.Send(new SelectedSavegameChangedEvent(savegame));
    }

    [RelayCommand]
    private void SelectSavegameDir()
    {
        WeakReferenceMessenger.Default.Send(new RequestSelectSavegameDirEvent());
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameDirChangedEvent>(this,
            (_, message) =>
            {
                SaveDir = message.NewPath;
                _savegames.Clear();
                foreach (var savegame in SavegameManager.GetSavegames()
                             .Values)
                    _savegames.Add(savegame);

                OnPropertyChanged(nameof(SinglePlayerSaves));
                OnPropertyChanged(nameof(MultiPlayerSaves));
                OnPropertyChanged(nameof(MultiPlayerClientSaves));
            });
    }
}