using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Storage;
using SOTFEdit.Model.Savegame;
using SOTFEdit.Model.Storage;
using SOTFEdit.View.Storage;

namespace SOTFEdit.ViewModel;

public partial class StoragePageViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<int, AdvancedStorageDefinition> _advancedStorageDefinitions;
    private readonly Dictionary<int, StorageDefinition> _storageDefinitions;
    private readonly StorageFactory _storageFactory;

    [ObservableProperty]
    private IStorage? _selectedStorage;

    [ObservableProperty]
    private UserControl? _selectedUserControl;

    public StoragePageViewModel(StorageFactory storageFactory, GameData gameData)
    {
        _storageFactory = storageFactory;
        StorageCollections = new ObservableCollection<StorageCollection>();
        _storageDefinitions = gameData.StorageDefinitions.ToDictionary(definition => definition.Id);
        _advancedStorageDefinitions = gameData.AdvancedStorageDefinitions.ToDictionary(definition => definition.Id);
        SetupListeners();
    }

    public ObservableCollection<StorageCollection> StorageCollections { get; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => OnSelectedSavegameChanged(m));
        WeakReferenceMessenger.Default.Register<ApplyToAllOfSameTypeEvent>(this,
            (_, m) => OnApplyToAllOfSameTypeEvent(m));
    }

    private void OnApplyToAllOfSameTypeEvent(ApplyToAllOfSameTypeEvent message)
    {
        var collection = StorageCollections.FirstOrDefault(collection =>
            collection.StorageTypeId == message.Storage.GetStorageTypeId());
        if (collection == null)
        {
            return;
        }

        foreach (var storage in collection.Storages.Where(s => s != message.Storage))
        {
            storage.ApplyFrom(message.Storage);
        }
    }

    private bool HasStorages()
    {
        return StorageCollections.Any(storageCollection => storageCollection.Storages.Count > 0);
    }

    [RelayCommand(CanExecute = nameof(HasStorages))]
    private void FillAllStorages()
    {
        foreach (var storageCollection in StorageCollections)
        foreach (var storage in storageCollection.Storages)
        {
            storage.SetAllToMax();
        }
    }

    [RelayCommand]
    private void SelectedItemChanged(object? item)
    {
        if (item is not IStorage iStorage)
        {
            return;
        }

        SelectedUserControl = iStorage switch
        {
            BaseStorage baseStorage => new ItemStorageUserControl(baseStorage),
            AdvancedItemsStorage advancedItemsStorage => new AdvancedItemStorageUserControl(advancedItemsStorage),
            _ => SelectedUserControl
        };
        SelectedStorage = iStorage;
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        SelectedUserControl = null;
        SelectedStorage = null;
        StorageCollections.Clear();

        if (message.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        if (ReadStorageCollections(selectedSavegame) is not { } storageCollectionsById)
        {
            return;
        }

        foreach (var storageCollection in storageCollectionsById.Values.OrderBy(collection => collection.Name))
        {
            StorageCollections.Add(storageCollection);
        }

        FillAllStoragesCommand.NotifyCanExecuteChanged();
    }

    private List<StorageSaveData>? LoadStorageSaveData(Savegame selectedSavegame)
    {
        if (selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureInstancesSaveData)
                ?.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances)?["_structures"] is not JArray
            structures)
        {
            return null;
        }

        var result = new List<StorageSaveData>();

        foreach (var structure in structures)
        {
            if (structure.ToObject<StorageSaveData>() is not { } storageSaveData || !StorageIdKnown(storageSaveData.Id))
            {
                continue;
            }

            result.Add(storageSaveData);
        }

        return result;
    }

    private bool StorageIdKnown(int id)
    {
        return _storageDefinitions.ContainsKey(id) || _advancedStorageDefinitions.ContainsKey(id);
    }

    private Dictionary<int, StorageCollection>? ReadStorageCollections(Savegame selectedSavegame)
    {
        if (LoadStorageSaveData(selectedSavegame) is not { } storageSaveData)
        {
            return null;
        }

        var storageCollectionsById = new Dictionary<int, StorageCollection>();

        foreach (var saveData in storageSaveData)
        {
            var storageDefinition = _storageDefinitions.GetValueOrDefault(saveData.Id) ??
                                    (IStorageDefinition?)_advancedStorageDefinitions.GetValueOrDefault(saveData.Id);
            if (storageDefinition == null)
            {
                continue;
            }

            StorageCollection storageCollection;

            if (!storageCollectionsById.ContainsKey(saveData.Id))
            {
                storageCollection = new StorageCollection(saveData.Id, storageDefinition.Name);
                storageCollectionsById.Add(saveData.Id, storageCollection);
            }
            else
            {
                storageCollection = storageCollectionsById[saveData.Id];
            }

            var storage = _storageFactory.Build(storageDefinition, storageCollection.Storages.Count + 1);
            storage.SetSaveData(saveData);

            storageCollection.Storages.Add(storage);
        }

        return storageCollectionsById;
    }

    public bool Update(Savegame savegame)
    {
        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureInstancesSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances)?["_structures"] is not JArray
                structures)
        {
            return false;
        }

        var newStorageSaveDataById = StorageCollections.SelectMany(collection => collection.Storages)
            .Select(storage => storage.ToStorageSaveData())
            .GroupBy(saveData => saveData.Id)
            .ToDictionary(group => group.Key, group => group.ToList());

        var numProcessedById = new Dictionary<int, int>();

        var hasChanges = false;

        foreach (var structureToken in structures)
        {
            if (structureToken["Id"]?.Value<int>() is not { } structureId || !StorageIdKnown(structureId))
            {
                continue;
            }

            if (newStorageSaveDataById.GetValueOrDefault(structureId) is not { } newStorageSaveData)
            {
                Logger.Warn(
                    $"No new storage save data found for id {structureId}, although it exists in original savegame. This might be a bug. Skip saving");
                return false;
            }

            var numProcessed = numProcessedById.GetValueOrDefault(structureId, 0);
            if (newStorageSaveData.Count <= numProcessed || newStorageSaveData[numProcessed] is not { } newSaveData)
            {
                Logger.Warn(
                    $"No new storage save data found for id {structureId} at index {numProcessed}, although it exists in original savegame. This might be a bug. Skip saving");
                return false;
            }

            if (structureToken.ToObject<StorageSaveData>() is not { } oldStorageSaveData)
            {
                Logger.Warn($"Unable to deserialize structure {structureId} at index {numProcessed}. Skip saving");
                return false;
            }

            if (oldStorageSaveData.Equals(newSaveData))
            {
                Logger.Info(
                    $"Storage save data for structure {structureId} at index {numProcessed} is equal, will not overwrite");
                numProcessedById[structureId] = numProcessed + 1;
                continue;
            }

            hasChanges = MergeIntoStorageBlock(structureToken, newSaveData) || hasChanges;
            numProcessedById[structureId] = numProcessed + 1;
        }

        if (hasChanges)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureInstances);
        }

        return hasChanges;
    }

    private static bool MergeIntoStorageBlock(JToken target, StorageSaveData newData)
    {
        if (target["Storages"] is not { } storagesToken)
        {
            return false;
        }

        storagesToken.Replace(JToken.FromObject(newData.Storages));
        return true;
    }
}