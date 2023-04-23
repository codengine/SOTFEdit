using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Storage;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public partial class StructuresPageViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly WpfObservableRangeCollection<ScrewStructureWrapper> _structures = new();

    private readonly List<ScrewStructure> _structureTypes;

    [ObservableProperty] private ScrewStructure? _batchSelectedStructureType;

    public StructuresPageViewModel(GameData gameData)
    {
        _structureTypes = gameData.ScrewStructures.OrderBy(screwStructure => screwStructure.CategoryName)
            .ThenBy(screwStructure => screwStructure.Name).ToList();
        _structureTypes.Insert(0, new ScrewStructure("", 0, 0, false));

        StructureTypes = new ListCollectionView(_structureTypes)
        {
            GroupDescriptions =
            {
                new PropertyGroupDescription("CategoryName")
            },
            SortDescriptions =
            {
                new SortDescription("CategoryName", ListSortDirection.Ascending),
                new SortDescription("Name", ListSortDirection.Ascending)
            }
        };
        Structures = new ListCollectionView(_structures)
        {
            GroupDescriptions =
            {
                new PropertyGroupDescription("Name")
            },
            SortDescriptions =
            {
                new SortDescription("Name", ListSortDirection.Ascending),
                new SortDescription("PctDone", ListSortDirection.Ascending)
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
        WeakReferenceMessenger.Default.Register<ChangeScrewStructureResult>(this,
            (_, message) => { OnChangeScrewStructureResult(message); });
    }

    private void OnChangeScrewStructureResult(ChangeScrewStructureResult message)
    {
        message.ScrewStructureWrapper.Update(message.SelectedScrewStructure);
        Structures.Refresh();
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
        var wrappers = new List<ScrewStructureWrapper>();
        wrappers.AddRange(LoadUnfinishedStructures(selectedSavegame));
        wrappers.AddRange(LoadFinishedStructures(selectedSavegame));
        return wrappers;
    }

    private IEnumerable<ScrewStructureWrapper> LoadFinishedStructures(Savegame selectedSavegame)
    {
        var wrappers = new List<ScrewStructureWrapper>();
        if (
            selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureInstancesSaveData) is
                not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances) is not
                { } screwStructureInstances || screwStructureInstances["_structures"] is not JArray structures
        )
        {
            return wrappers;
        }

        var screwStructuresById = _structureTypes.ToDictionary(screwStructure => screwStructure.Id);

        foreach (var structure in structures)
        {
            ScrewStructure? screwStructure = null;
            if (structure["Id"]?.Value<int>() is { } id)
            {
                screwStructure = screwStructuresById.GetValueOrDefault(id);
            }

            if (screwStructure == null)
            {
                continue;
            }

            var position = structure["Pos"]?.ToObject<Position>() ?? null;
            wrappers.Add(new ScrewStructureWrapper(screwStructure, structure, screwStructure.BuildCost, position,
                ScrewStructureOrigin.Finished));
        }

        return wrappers;
    }

    private IEnumerable<ScrewStructureWrapper> LoadUnfinishedStructures(Savegame selectedSavegame)
    {
        var wrappers = new List<ScrewStructureWrapper>();
        if (
            selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureNodeInstancesSaveData) is
                not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureNodeInstances) is not
                { } screwStructureNodeInstances || screwStructureNodeInstances["_structures"] is not JArray structures
        )
        {
            return wrappers;
        }

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
            wrappers.Add(new ScrewStructureWrapper(screwStructure, structure, added, position,
                ScrewStructureOrigin.Unfinished));
        }

        return wrappers;
    }

    public bool Update(Savegame savegame)
    {
        var toBeModifiedCount = _structures.Count(structure =>
            structure.ModificationMode != null && structure.ModificationMode != ScrewStructureModificationMode.None);

        if (toBeModifiedCount == 0)
        {
            return false;
        }
        
        if (
            savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureNodeInstancesSaveData) is
                not { } screwStructureNodeInstancesSaveWrapper ||
            screwStructureNodeInstancesSaveWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureNodeInstances) is not
                { } screwStructureNodeInstances
        )
        {
            Logger.Info("ScrewStructureNodeInstances not found, will skip saving structures");
            return false;
        }
        
        if (
            savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureInstancesSaveData) is
                not { } screwStructureInstancesSaveDataWrapper ||
            screwStructureInstancesSaveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances) is not
                { } screwStructureInstances
        )
        {
            Logger.Info("ScrewStructureInstances not found, will skip saving structures");
            return false;
        }

        var unfinishedStructures = new JArray();
        var finishedStructures = new JArray();

        foreach (var wrapper in _structures)
        {
            if (wrapper.ModificationMode == ScrewStructureModificationMode.Remove)
            {
                continue;
            }

            if (wrapper.Origin == ScrewStructureOrigin.Unfinished)
            {
                ProcessUnfinished(wrapper, unfinishedStructures, finishedStructures);
            }
            else if (wrapper.Origin == ScrewStructureOrigin.Finished)
            {
                ProcessFinished(wrapper, unfinishedStructures, finishedStructures);
            }
        }

        screwStructureNodeInstances["_structures"] = unfinishedStructures;
        screwStructureNodeInstancesSaveWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureNodeInstances);
        
        screwStructureInstances["_structures"] = finishedStructures;
        screwStructureInstancesSaveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureInstances);

        return true;
    }

    private static void ProcessUnfinished(ScrewStructureWrapper wrapper, JArray unfinishedStructures,
        JArray finishedStructures)
    {
        var token = wrapper.Token.DeepClone();
        var id = wrapper.ChangedTypeId ?? token["Id"]?.Value<int>();
        if (id != null)
        {
            token["Id"] = id;
        }

        if (wrapper.ModificationMode is ScrewStructureModificationMode.None or null || id is not {} theId)
        {
            unfinishedStructures.Add(token);
            return;
        }
        
        if (wrapper.ModificationMode == ScrewStructureModificationMode.Finish)
        {
            finishedStructures.Add(JToken.FromObject(new StorageSaveData
            {
                Id = theId,
                Pos = wrapper.Position,
                Rot = token["Rot"]
            }));

            return;
        }

        var added = wrapper.ModificationMode switch
        {
            ScrewStructureModificationMode.AlmostFinish => wrapper.BuildCost == 1 ? 0 : wrapper.BuildCost - 1,
            ScrewStructureModificationMode.Unfinish => 0,
            _ => wrapper.Added
        };

        token["Added"] = added;
        unfinishedStructures.Add(token);
    }
    
    private static void ProcessFinished(ScrewStructureWrapper wrapper, JArray unfinishedStructures, JArray finishedStructures)
    {
        var token = wrapper.Token.DeepClone();
        
        var id = wrapper.ChangedTypeId ?? token["Id"]?.Value<int>();
        if (id != null)
        {
            token["Id"] = id;
        }

        if (wrapper.ModificationMode is ScrewStructureModificationMode.None or null || id is not {} theId || wrapper.Position is not {} position || token["Rot"] is not {} rotation)
        {
            finishedStructures.Add(token);
            return;
        }

        var added = wrapper.ModificationMode switch
        {
            ScrewStructureModificationMode.AlmostFinish => wrapper.BuildCost == 1 ? 0 : wrapper.BuildCost - 1,
            ScrewStructureModificationMode.Unfinish => 0,
            _ => 0
        };

        unfinishedStructures.Add(JToken.FromObject(new UnfinishedGenericScrewStructure(theId, position, rotation, added)));
    }

    [RelayCommand(CanExecute = nameof(HasSavegameSelected))]
    private void SetModificationMode(ScrewStructureModificationWrapper modificationWrapper)
    {
        foreach (var wrapper in _structures.Where(structure => structure.Origin == modificationWrapper.Origin).Where(
                     structure =>
                         BatchSelectedStructureType == null || BatchSelectedStructureType.Name == "" ||
                         structure.ScrewStructure?.Id == BatchSelectedStructureType.Id))
        {
            wrapper.ModificationMode = modificationWrapper.ModificationMode;
        }
    }

    private static bool HasSavegameSelected()
    {
        return SavegameManager.SelectedSavegame != null;
    }
}