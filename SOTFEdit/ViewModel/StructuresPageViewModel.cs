using System;
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

    private readonly List<ScrewStructure> _structureTypes;

    [ObservableProperty]
    private ScrewStructure? _batchSelectedStructureType;

    public StructuresPageViewModel(GameData gameData)
    {
        _structureTypes = gameData.ScrewStructures.OrderBy(screwStructure => screwStructure.CategoryName)
            .ThenBy(screwStructure => screwStructure.Name).ToList();
        _structureTypes.Insert(0, new ScrewStructure("", 0, 0, false, "", false, false, false));

        StructureTypes = CollectionViewSource.GetDefaultView(_structureTypes);
        StructureTypes.GroupDescriptions.Add(new PropertyGroupDescription("CategoryName"));
        StructureTypes.SortDescriptions.Add(new SortDescription("CategoryName", ListSortDirection.Ascending));
        StructureTypes.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

        StructureView = CollectionViewSource.GetDefaultView(Structures);
        StructureView.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
        StructureView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        StructureView.SortDescriptions.Add(new SortDescription("PctDone", ListSortDirection.Ascending));

        SetupListeners();
    }

    public ObservableCollectionEx<ScrewStructureWrapper> Structures { get; } = new();

    public ICollectionView StructureTypes { get; }

    public ICollectionView StructureView { get; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, message) => OnSelectedSavegameChangedEvent(message));
        WeakReferenceMessenger.Default.Register<ChangeScrewStructureResult>(this,
            (_, message) => OnChangeScrewStructureResult(message));
    }

    private void OnChangeScrewStructureResult(ChangeScrewStructureResult message)
    {
        message.ScrewStructureWrapper.Update(message.SelectedScrewStructure);
        StructureView.Refresh();
    }

    private void OnSelectedSavegameChangedEvent(SelectedSavegameChangedEvent message)
    {
        Structures.Clear();
        if (message.SelectedSavegame is { } selectedSavegame)
        {
            Structures.AddRange(LoadStructures(selectedSavegame).OrderBy(wrapper => wrapper.Category)
                .ThenBy(wrapper => wrapper.Name));
        }

        StructureView.Refresh();
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

        for (var i = 0; i < structures.Count; i++)
        {
            var structure = structures[i];
            if (structure.Type == JTokenType.Null)
            {
                continue;
            }

            ScrewStructure? screwStructure = null;

            var screwStructureId = structure["Id"]?.Value<int>();
            if (screwStructureId is { } id)
            {
                screwStructure = screwStructuresById.GetValueOrDefault(id);
            }

            if (screwStructure == null && screwStructureId != null)
            {
                Logger.Debug($"Unknown finished Screw Structure: {screwStructureId}");
            }

            var position = structure["Pos"]?.ToObject<Position>() ?? null;
            wrappers.Add(new ScrewStructureWrapper(screwStructure, structure, screwStructure?.BuildCost ?? 0, position,
                ScrewStructureOrigin.Finished, i));
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

        for (var i = 0; i < structures.Count; i++)
        {
            var structure = structures[i];
            ScrewStructure? screwStructure = null;
            var screwStructureId = structure["Id"]?.Value<int>();
            if (screwStructureId is { } id)
            {
                screwStructure = screwStructuresById.GetValueOrDefault(id);
            }

            var added = structure["Added"]?.Value<int>() ?? 0;
            var position = structure["Pos"]?.ToObject<Position>() ?? null;

            if (screwStructure == null && screwStructureId != null)
            {
                Logger.Debug($"Unknown unfinished Screw Structure: {screwStructureId}, added: {added}");
            }

            wrappers.Add(new ScrewStructureWrapper(screwStructure, structure, added, position,
                ScrewStructureOrigin.Unfinished, i));
        }

        return wrappers;
    }

    public bool Update(Savegame savegame)
    {
        var toBeModifiedCount = Structures.Count(structure =>
            structure.ModificationMode != null && structure.ModificationMode != ScrewStructureModificationMode.None);

        if (toBeModifiedCount == 0)
        {
            return false;
        }

        if (
            savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureNodeInstancesSaveData) is
                not { } screwStructureNodeInstancesSaveWrapper ||
            screwStructureNodeInstancesSaveWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureNodeInstances) is
                not
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

        foreach (var wrapper in Structures.OrderBy(wrapper => wrapper.Index))
        {
            if (wrapper.ModificationMode == ScrewStructureModificationMode.Remove)
            {
                continue;
            }

            switch (wrapper.Origin)
            {
                case ScrewStructureOrigin.Unfinished:
                    ProcessUnfinished(wrapper, unfinishedStructures, finishedStructures);
                    break;
                case ScrewStructureOrigin.Finished:
                    ProcessFinished(wrapper, unfinishedStructures, finishedStructures);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(savegame), wrapper.Origin,
                        $"Unexpected {nameof(ScrewStructureOrigin)}");
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

        if (wrapper.ModificationMode is ScrewStructureModificationMode.None or null || id is not { } theId)
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

    private static void ProcessFinished(ScrewStructureWrapper wrapper, JArray unfinishedStructures,
        JArray finishedStructures)
    {
        var token = wrapper.Token.DeepClone();

        var id = wrapper.ChangedTypeId ?? token["Id"]?.Value<int>();
        if (id != null)
        {
            token["Id"] = id;
        }

        if (wrapper.ModificationMode is ScrewStructureModificationMode.None or null || id is not { } theId ||
            wrapper.Position is not { } position || token["Rot"] is not { } rotation)
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

        unfinishedStructures.Add(
            JToken.FromObject(new UnfinishedGenericScrewStructure(theId, position, rotation, added)));
    }

    [RelayCommand(CanExecute = nameof(HasSavegameSelected))]
    private void SetModificationMode(ScrewStructureModificationWrapper modificationWrapper)
    {
        foreach (var wrapper in Structures.Where(structure => structure.Origin == modificationWrapper.Origin).Where(
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