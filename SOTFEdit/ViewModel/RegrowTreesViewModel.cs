using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public partial class RegrowTreesViewModel : ObservableObject
{
    private readonly ICloseable _parent;
    private int _countGone;
    private int _countHalfChopped;
    private int _countStumps;

    [ObservableProperty]
    private int _pctRegrow = 100;

    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(VegetationStateIsAllSelected))]
    [ObservableProperty]
    private VegetationState _vegetationStateSelected =
        VegetationState.Gone | VegetationState.HalfChopped | VegetationState.Stumps;

    public RegrowTreesViewModel(ICloseable parent, Savegame selectedSavegame)
    {
        _parent = parent;
        LoadTreeCounts(selectedSavegame);
    }

    public string PrintAll =>
        $"{TranslationManager.Get("windows.regrowTrees.all")} ({_countGone + _countStumps + _countHalfChopped})";

    public string PrintStumps => $"{TranslationManager.Get("windows.regrowTrees.stumps")} ({_countStumps})";

    public string PrintHalfChopped =>
        $"{TranslationManager.Get("windows.regrowTrees.halfChopped")} ({_countHalfChopped})";

    public string PrintGone => $"{TranslationManager.Get("windows.regrowTrees.gone")} ({_countGone})";

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

    private void LoadTreeCounts(Savegame selectedSavegame)
    {
        if (selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WorldObjectLocatorManagerSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.WorldObjectLocatorManager) is not
                { } worldObjectLocatorManager)
        {
            return;
        }

        var serializedStates = worldObjectLocatorManager["SerializedStates"]?.ToList() ?? Enumerable.Empty<JToken>();

        foreach (var serializedState in serializedStates)
        {
            var valueToken = serializedState["Value"];
            var value = valueToken?.Value<short>();
            if (value == null)
            {
                continue;
            }

            var shiftedValue = (short)(1 << (value - 1));

            if ((shiftedValue & (int)VegetationState.Gone) != 0)
            {
                _countGone++;
            }

            if ((shiftedValue & (int)VegetationState.HalfChopped) != 0)
            {
                _countHalfChopped++;
            }

            if ((shiftedValue & (int)VegetationState.Stumps) != 0)
            {
                _countStumps++;
            }
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

        var countRegrown = selectedSavegame.RegrowTrees(VegetationStateSelected, _pctRegrow);

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