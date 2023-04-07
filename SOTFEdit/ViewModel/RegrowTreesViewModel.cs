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

        var resultMessage =
            countRegrown == 0
                ? $"No trees with state \"{VegetationStateSelected}\" regrown"
                : $"{countRegrown} trees with previous state \"{VegetationStateSelected}\" should now have regrown. Please save to persist the changes.";
        WeakReferenceMessenger.Default.Send(new GenericMessageEvent(resultMessage, "Regrow Trees"));
        _parent.Close();
    }

    private bool CanRegrowTrees()
    {
        return VegetationStateSelected != VegetationState.None;
    }
}