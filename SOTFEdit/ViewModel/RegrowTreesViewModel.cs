using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;

namespace SOTFEdit.ViewModel;

public partial class RegrowTreesViewModel : ObservableObject
{
    private readonly ICloseable _parent;

    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(VegetationStateIsAllSelected))]
    [ObservableProperty]
    private VegetationState _vegetationStateSelected =
        VegetationState.Gone | VegetationState.HalfChopped | VegetationState.Stumps;

    public RegrowTreesViewModel(ICloseable parent)
    {
        _parent = parent;
    }

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

    [RelayCommand(CanExecute = nameof(CanRegrowTrees))]
    private void Save()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            _parent.Close();
            return;
        }

        var countRegrown = selectedSavegame.RegrowTrees(VegetationStateSelected);

        var statesPrintable = new List<string>();
        if ((VegetationStateSelected & VegetationState.Gone) != 0)
        {
            statesPrintable.Add(TranslationManager.Get("windows.regrowTrees.gone"));
        }

        if ((VegetationStateSelected & VegetationState.HalfChopped) != 0)
        {
            statesPrintable.Add(TranslationManager.Get("windows.regrowTrees.halfChopped"));
        }

        if ((VegetationStateSelected & VegetationState.Stumps) != 0)
        {
            statesPrintable.Add(TranslationManager.Get("windows.regrowTrees.stumps"));
        }

        var resultMessage =
            countRegrown == 0
                ? TranslationManager.GetFormatted("windows.regrowTrees.messages.noTreesRegrown",
                    string.Join(", ", statesPrintable))
                : TranslationManager.GetFormatted("windows.regrowTrees.messages.success", countRegrown,
                    string.Join(", ", statesPrintable));
        WeakReferenceMessenger.Default.Send(new GenericMessageEvent(resultMessage,
            TranslationManager.Get("windows.regrowTrees.title")));
        _parent.Close();
    }

    private bool CanRegrowTrees()
    {
        return VegetationStateSelected != VegetationState.None;
    }
}