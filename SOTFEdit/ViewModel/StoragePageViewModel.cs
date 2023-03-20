using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Storage;
using SOTFEdit.Model.Storage;
using SOTFEdit.View.Storage;

namespace SOTFEdit.ViewModel;

public partial class StoragePageViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly Dictionary<int, StorageDefinition> _storageDefinitions;
    private readonly StorageFactory _storageFactory;
    [ObservableProperty] private UserControl? _selected;

    public StoragePageViewModel(StorageFactory storageFactory, GameData gameData)
    {
        _storageFactory = storageFactory;
        StorageCollections = new ObservableCollection<StorageCollection>();
        _storageDefinitions = gameData.StorageDefinitions.ToDictionary(definition => definition.Id);
        SetupListeners();
    }

    public ObservableCollection<StorageCollection> StorageCollections { get; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    [RelayCommand]
    public void SelectedItemChanged(object? item)
    {
        if (item is not IStorage)
        {
            return;
        }

        Selected = item switch
        {
            ItemsStorage itemsStorage => new ItemStorageUserControl(itemsStorage),
            StorageWithModulePerItem logStorage => new ItemStorageUserControl(logStorage),
            FoodStorage foodStorage => new ItemStorageUserControl(foodStorage),
            _ => Selected
        };
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        Selected = null;
        StorageCollections.Clear();

        if (message.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        if (ReadStorageCollections(selectedSavegame) is not { } storageCollectionsById)
        {
            return;
        }

        foreach (var storageCollection in storageCollectionsById.Values)
        {
            StorageCollections.Add(storageCollection);
        }
    }

    private List<StorageSaveData>? LoadStorageSaveData(Savegame selectedSavegame)
    {
        var screwStructureJson =
            selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureInstancesSaveData)?["Data"]?
                ["ScrewStructureInstances"]?.ToObject<string>();

        if (screwStructureJson is not { } || JsonConverter.DeserializeRaw(screwStructureJson) is not
                { } screwStructure || screwStructure["_structures"] is not JArray structures)
        {
            return null;
        }

        var result = new List<StorageSaveData>();

        foreach (var structure in structures)
        {
            if (structure.ToObject<StorageSaveData>() is not { } storageSaveData ||
                !_storageDefinitions.ContainsKey(storageSaveData.Id))
            {
                continue;
            }

            result.Add(storageSaveData);
        }

        return result;
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
            if (_storageDefinitions.GetValueOrDefault(saveData.Id) is not { } storageDefinition)
            {
                continue;
            }

            StorageCollection storageCollection;

            if (!storageCollectionsById.ContainsKey(saveData.Id))
            {
                storageCollection = new StorageCollection(storageDefinition);
                storageCollectionsById.Add(saveData.Id, storageCollection);
            }
            else
            {
                storageCollection = storageCollectionsById[saveData.Id];
            }

            var storage = _storageFactory.Build(storageDefinition, storageCollection.Storages.Count + 1);
            storage.SetItemsFromJson(saveData);

            storageCollection.Storages.Add(storage);
        }

        return storageCollectionsById;
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureInstancesSaveData) is not
            { } screwStructureInstancesSaveData)
        {
            return false;
        }

        var screwStructureInstancesToken = screwStructureInstancesSaveData["Data"]?["ScrewStructureInstances"];

        if (screwStructureInstancesToken?.ToObject<string>() is not { } screwStructureJson ||
            JsonConverter.DeserializeRaw(screwStructureJson) is not
                { } screwStructure || screwStructure["_structures"] is not JArray structures)
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
            if (structureToken["Id"]?.ToObject<int>() is not { } structureId ||
                !_storageDefinitions.ContainsKey(structureId))
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

        if (!hasChanges)
        {
            return hasChanges;
        }

        screwStructureInstancesToken.Replace(JsonConverter.Serialize(screwStructure));
        savegame.SavegameStore.StoreJson(SavegameStore.FileType.ScrewStructureInstancesSaveData,
            screwStructureInstancesSaveData, createBackup);

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

public class StorageCollection
{
    public StorageCollection(StorageDefinition storageDefinition)
    {
        StorageDefinition = storageDefinition;
    }

    public StorageDefinition StorageDefinition { get; }
    public ObservableCollection<IStorage> Storages { get; } = new();
}