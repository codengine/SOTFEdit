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

    public SelectSavegameViewModel(SavegameManager savegameManager)
    {
        _savegames = new List<Savegame>(savegameManager.GetSavegames().Values);

        SetupListeners(savegameManager);
    }

    public List<Savegame> SinglePlayerSaves => _savegames
        .Where(savegame => savegame.IsSinglePlayer())
        .ToList();

    public List<Savegame> MultiPlayerSaves => _savegames
        .Where(savegame => savegame.IsMultiPlayer())
        .ToList();

    public List<Savegame> MultiPlayerClientSaves => _savegames
        .Where(savegame => savegame.IsMultiPlayerClient())
        .ToList();

    public string SaveDir => SavegameManager.GetSavePath();

    [RelayCommand]
    public void SelectSavegame(Savegame savegame)
    {
        WeakReferenceMessenger.Default.Send(new SelectedSavegameChangedEvent(savegame));
    }

    [RelayCommand]
    public void SelectSavegameDir()
    {
        WeakReferenceMessenger.Default.Send(new RequestSelectSavegameDirEvent());
    }

    private void SetupListeners(SavegameManager savegameManager)
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameDirChangedEvent>(this,
            (_, _) =>
            {
                OnPropertyChanged(nameof(SaveDir));
                _savegames.Clear();
                foreach (var savegame in savegameManager.GetSavegames()
                             .Values)
                {
                    _savegames.Add(savegame);
                }

                OnPropertyChanged(nameof(SinglePlayerSaves));
                OnPropertyChanged(nameof(MultiPlayerSaves));
                OnPropertyChanged(nameof(MultiPlayerClientSaves));
            });
    }
}