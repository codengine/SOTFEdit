using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public partial class StructuresPageViewModel : ObservableObject
{
    private readonly WpfObservableRangeCollection<ScrewStructureWrapper> _structures = new();

    private readonly List<ScrewStructure> _structureTypes;

    [ObservableProperty] private ScrewStructure? _batchSelectedStructureType;

    public StructuresPageViewModel(GameData gameData)
    {
        _structureTypes = gameData.ScrewStructures.OrderBy(screwStructure => screwStructure.Category)
            .ThenBy(screwStructure => screwStructure.Name).ToList();
        _structureTypes.Insert(0, new ScrewStructure("", "", 0, 0));

        StructureTypes = new ListCollectionView(_structureTypes)
        {
            GroupDescriptions =
            {
                new PropertyGroupDescription("Category")
            }
        };
        Structures = new ListCollectionView(_structures)
        {
            GroupDescriptions =
            {
                new PropertyGroupDescription("Category")
            }
        };
        SetupListeners();
    }

    public ListCollectionView StructureTypes { get; }

    public ICollectionView Structures { get; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, message) => OnSelectedSavegameChangedEvent(message));
    }

    private void OnSelectedSavegameChangedEvent(SelectedSavegameChangedEvent message)
    {
        _structures.Clear();
        if (message.SelectedSavegame is { } selectedSavegame)
        {
            _structures.AddRange(LoadStructures(selectedSavegame).OrderBy(wrapper => wrapper.Category)
                .ThenBy(wrapper => wrapper.Name));
        }

        Structures.Refresh();
        SetModificationModeCommand.NotifyCanExecuteChanged();
    }

    private IEnumerable<ScrewStructureWrapper> LoadStructures(Savegame selectedSavegame)
    {
        if (
            selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureNodeInstancesSaveData) is
                not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureNodeInstances) is not
                { } screwStructureNodeInstances || screwStructureNodeInstances["_structures"] is not JArray structures
        )
        {
            return Enumerable.Empty<ScrewStructureWrapper>();
        }

        var wrappers = new List<ScrewStructureWrapper>();

        var screwStructuresById = _structureTypes.ToDictionary(screwStructure => screwStructure.Id);

        foreach (var structure in structures)
        {
            ScrewStructure? screwStructure = null;
            if (structure["Id"]?.Value<int>() is { } id)
            {
                screwStructure = screwStructuresById.GetValueOrDefault(id);
            }

            var added = structure["Added"]?.Value<int>() ?? 0;
            var position = structure["Pos"]?.ToObject<Position>() ?? null;
            wrappers.Add(new ScrewStructureWrapper(screwStructure, structure, added, position));
        }

        return wrappers;
    }

    public bool Update(Savegame savegame)
    {
        var toBeModifiedCount = _structures.Count(structure => !string.IsNullOrEmpty(structure.ModificationMode));

        if (toBeModifiedCount == 0)
        {
            return false;
        }

        if (
            savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureNodeInstancesSaveData) is
                not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureNodeInstances) is not
                { } screwStructureNodeInstances
        )
        {
            return false;
        }

        var jArray = new JArray();

        foreach (var wrapper in _structures)
        {
            if (wrapper.ModificationMode == "Remove")
            {
                continue;
            }

            var token = wrapper.Token.DeepClone();

            if (!string.IsNullOrEmpty(wrapper.ModificationMode))
            {
                var added = 0;

                if (wrapper.ModificationMode == "Finish")
                {
                    added = wrapper.BuildCost == 1 ? 0 : wrapper.BuildCost - 1;
                }

                token["Added"] = added;
            }

            jArray.Add(token);
        }

        screwStructureNodeInstances["_structures"] = jArray;

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureNodeInstances);

        return true;
    }

    [RelayCommand(CanExecute = nameof(HasSavegameSelected))]
    private void SetModificationMode(string? mode)
    {
        foreach (var wrapper in _structures.Where(structure =>
                     BatchSelectedStructureType == null || BatchSelectedStructureType.Name == "" ||
                     structure.Category == BatchSelectedStructureType.Category))
            wrapper.ModificationMode = mode;
    }

    private bool HasSavegameSelected()
    {
        return SavegameManager.SelectedSavegame != null;
    }
}