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

public partial class ItemPlaterViewModel : ObservableObject
{
    private readonly Dictionary<ElementProfileCategory, List<ElementProfile>> _elementProfileCategories;
    private readonly ICloseable _parent;

    private readonly Dictionary<int, Item> _platableItems;

    private readonly Dictionary<string, HashSet<int>> _screwStructureCategoryToIds;

    private readonly HashSet<int> _weaponHolderScrewStructureIds;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _followerItems = true;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _furnitureStructures;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _gardeningStructures;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _itemConstruction;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _itemsInInventory = true;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _itemsInWeaponRacks = true;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _logConstruction;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _logPlankConstruction;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _miscStructures;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _stickConstruction;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _stoneConstruction;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _storageStructures;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _trapsStructures;

    [NotifyCanExecuteChangedFor(nameof(AddPlatingCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemovePlatingCommand))]
    [ObservableProperty]
    private bool _utilityStructures;

    public ItemPlaterViewModel(ICloseable parent, GameData gameData)
    {
        _parent = parent;
        _platableItems = gameData.Items.Where(item => item.Value.IsPlatable)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        _weaponHolderScrewStructureIds = gameData.ScrewStructures.Where(screwStructure => screwStructure.IsWeaponHolder)
            .Select(screwStructure => screwStructure.Id)
            .ToHashSet();
        _screwStructureCategoryToIds =
            gameData.ScrewStructures.GroupBy(structure => structure.Category, structure => structure.Id)
                .ToDictionary(ints => ints.Key, ints => ints.ToHashSet());
        _elementProfileCategories =
            gameData.ElementProfiles.GroupBy(profile => profile.Category)
                .ToDictionary(profiles => profiles.Key, profiles => profiles.ToList());
    }

    private bool CanModifyPlating()
    {
        return ItemsInInventory || ItemsInWeaponRacks;
    }

    [RelayCommand(CanExecute = nameof(CanModifyPlating))]
    private void AddPlating()
    {
        var modifiedCount = 0;

        var wrappersToSave = new Dictionary<SavegameStore.FileType, SaveDataWrapper>();

        if (ItemsInInventory && GetPlayerInventorySaveDataWrapper() is { } playerInventorySaveDataWrapper)
        {
            var inventoryPlatings = AddPlatingForInventory(playerInventorySaveDataWrapper);
            if (inventoryPlatings > 0)
            {
                wrappersToSave.TryAdd(SavegameStore.FileType.PlayerInventorySaveData, playerInventorySaveDataWrapper);
            }

            modifiedCount += inventoryPlatings;
        }

        if (ItemsInWeaponRacks && GetScrewStructuresSaveDataWrapper() is { } screwStructuresSaveDataWrapper)
        {
            var weaponRackPlatings = AddPlatingInWeaponRacks(screwStructuresSaveDataWrapper);
            if (weaponRackPlatings > 0)
            {
                wrappersToSave.TryAdd(SavegameStore.FileType.ScrewStructureInstancesSaveData,
                    screwStructuresSaveDataWrapper);
            }

            modifiedCount += weaponRackPlatings;
        }

        if (FollowerItems && GetVailSaveDataWrapper() is { } vailSaveDataWrapper)
        {
            var followerItemPlatings = AddPlatingToFollowers(vailSaveDataWrapper);
            if (followerItemPlatings > 0)
            {
                wrappersToSave.TryAdd(SavegameStore.FileType.SaveData, vailSaveDataWrapper);
            }

            modifiedCount += followerItemPlatings;
        }

        if ((StorageStructures || FurnitureStructures || TrapsStructures || UtilityStructures || MiscStructures ||
             GardeningStructures) &&
            GetScrewStructuresSaveDataWrapper() is { } screwStructuresSaveDataWrapper2)
        {
            var structurePlatings = AddPlatingToStructures(screwStructuresSaveDataWrapper2);
            if (structurePlatings > 0)
            {
                wrappersToSave.TryAdd(SavegameStore.FileType.ScrewStructureInstancesSaveData,
                    screwStructuresSaveDataWrapper2);
            }

            modifiedCount += structurePlatings;
        }

        if ((LogConstruction || LogPlankConstruction || StickConstruction || StoneConstruction || ItemConstruction) &&
            GetConstructionsSaveDataWrapper() is { } constructionsSaveDataWrapper)
        {
            var constructionPlatings = AddPlatingToConstructions(constructionsSaveDataWrapper);
            if (constructionPlatings > 0)
            {
                wrappersToSave.TryAdd(SavegameStore.FileType.ConstructionsSaveData,
                    constructionsSaveDataWrapper);
            }

            modifiedCount += constructionPlatings;
        }

        if (wrappersToSave.Count > 0)
        {
            var selectedSavegame = SavegameManager.SelectedSavegame!;
            WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(selectedSavegame, backupMode =>
            {
                selectedSavegame.SavegameStore.SaveWrappers(backupMode, wrappersToSave);

                ShowSuccessMessage(modifiedCount, "windows.itemPlater.messages.addPlating");
            }));
        }
        else
        {
            ShowSuccessMessage(modifiedCount, "windows.itemPlater.messages.addPlating");
        }

        _parent.Close();
    }

    private int AddPlatingToConstructions(SaveDataWrapper saveDataWrapper)
    {
        var numPlated = 0;

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.Constructions) is not
                { } constructions || constructions["Structures"] is not JArray structures)
        {
            return numPlated;
        }

        var selectedUnplatedProfilesById = GetSelectedCategoryProfileIds(false);

        foreach (var structure in structures)
        {
            if (structure is not JArray strucArray)
            {
                continue;
            }

            foreach (var strucBlock in strucArray)
            {
                if (strucBlock["Elements"] is not JArray elements)
                {
                    continue;
                }

                foreach (var element in elements)
                {
                    if (element["ProfileID"]?.Value<int>() is not { } profileId ||
                        !selectedUnplatedProfilesById.TryGetValue(profileId, out var profile))
                    {
                        continue;
                    }

                    numPlated++;
                    element["ProfileID"] = profile.GoldPlatedId;
                }
            }
        }

        if (numPlated > 0)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.Constructions);
        }

        return numPlated;
    }

    private int AddPlatingToStructures(SaveDataWrapper saveDataWrapper)
    {
        var numPlated = 0;

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances) is not
                { } screwStructureInstances || screwStructureInstances["_structures"] is not JArray structures)
        {
            return numPlated;
        }

        var selectedStructureTypeIds = GetSelectedStructureTypeIds();

        foreach (var structure in structures)
        {
            if (structure["Id"]?.Value<int>() is not { } id || !selectedStructureTypeIds.Contains(id))
            {
                continue;
            }

            if (structure["Modules"] is not JArray modules)
            {
                modules = new JArray();
                structure["Modules"] = modules;
            }

            if (FindStructurePlatingModule(modules) != null)
            {
                continue;
            }

            modules.Add(CreateStructurePlatingModule());
            numPlated++;
        }

        if (numPlated > 0)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureInstances);
        }

        return numPlated;
    }

    private Dictionary<int, ElementProfile> GetSelectedCategoryProfileIds(bool byPlatedId)
    {
        var result = new List<ElementProfile>();
        if (LogConstruction)
        {
            result.AddRange(
                _elementProfileCategories.GetValueOrDefault(ElementProfileCategory.Log, new List<ElementProfile>()));
        }

        if (LogPlankConstruction)
        {
            result.AddRange(_elementProfileCategories.GetValueOrDefault(ElementProfileCategory.LogPlank,
                new List<ElementProfile>()));
        }

        if (StickConstruction)
        {
            result.AddRange(_elementProfileCategories.GetValueOrDefault(ElementProfileCategory.Stick,
                new List<ElementProfile>()));
        }

        if (ItemConstruction)
        {
            result.AddRange(
                _elementProfileCategories.GetValueOrDefault(ElementProfileCategory.Item, new List<ElementProfile>()));
        }

        if (StoneConstruction)
        {
            result.AddRange(_elementProfileCategories.GetValueOrDefault(ElementProfileCategory.Stone,
                new List<ElementProfile>()));
        }

        return result.DistinctBy(KeySelector).ToDictionary(KeySelector);

        int KeySelector(ElementProfile profile)
        {
            return byPlatedId ? profile.GoldPlatedId : profile.Id;
        }
    }

    private ISet<int> GetSelectedStructureTypeIds()
    {
        var result = new List<int>();
        if (StorageStructures)
        {
            result.AddRange(_screwStructureCategoryToIds.GetValueOrDefault("storage", new HashSet<int>()));
        }

        if (FurnitureStructures)
        {
            result.AddRange(_screwStructureCategoryToIds.GetValueOrDefault("furniture", new HashSet<int>()));
        }

        if (TrapsStructures)
        {
            result.AddRange(_screwStructureCategoryToIds.GetValueOrDefault("traps", new HashSet<int>()));
        }

        if (UtilityStructures)
        {
            result.AddRange(_screwStructureCategoryToIds.GetValueOrDefault("utility", new HashSet<int>()));
        }

        if (MiscStructures)
        {
            result.AddRange(_screwStructureCategoryToIds.GetValueOrDefault("misc", new HashSet<int>()));
        }

        if (GardeningStructures)
        {
            result.AddRange(_screwStructureCategoryToIds.GetValueOrDefault("gardening", new HashSet<int>()));
        }

        return new HashSet<int>(result);
    }

    private static JObject CreateStructurePlatingModule()
    {
        return new JObject
        {
            ["StateDataTypeId"] = 2
        };
    }

    private static JToken? FindStructurePlatingModule(JArray modules)
    {
        return modules.FirstOrDefault(module => module["StateDataTypeId"]?.Value<int>() == 2);
    }

    private int AddPlatingToFollowers(SaveDataWrapper saveDataWrapper)
    {
        var numPlated = 0;

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.NpcItemInstances) is not
                { } npcItemInstances || npcItemInstances["ActorItems"] is not JArray actorItems)
        {
            return numPlated;
        }

        foreach (var actorData in actorItems)
        {
            if (actorData["Items"] is not JObject items || items["ItemBlocks"] is not JArray itemBlocks)
            {
                continue;
            }

            numPlated += AddPlatings(itemBlocks, false);
        }

        if (numPlated > 0)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.NpcItemInstances);
        }

        return numPlated;
    }

    private static void ShowSuccessMessage(int modifiedCount, string messageKey)
    {
        WeakReferenceMessenger.Default.Send(
            new GenericMessageEvent(
                TranslationManager.GetFormatted(messageKey, modifiedCount),
                TranslationManager.Get("generic.success")));
    }

    private int AddPlatingInWeaponRacks(SaveDataWrapper saveDataWrapper)
    {
        var numPlated = 0;

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances) is not
                { } screwStructureInstances || screwStructureInstances["_structures"] is not JArray structures)
        {
            return numPlated;
        }

        var weaponRacks = from structure in structures
            let structureId = structure["Id"]?.Value<int>()
            where structureId.HasValue && _weaponHolderScrewStructureIds.Contains(structureId.Value)
            select structure;

        foreach (var weaponRack in weaponRacks)
        {
            if (weaponRack["Storages"] is not JArray storages)
            {
                continue;
            }

            foreach (var storage in storages)
            {
                if (storage["ItemBlocks"] is not JArray itemBlocks)
                {
                    continue;
                }

                numPlated += AddPlatings(itemBlocks, false);
            }
        }

        if (numPlated > 0)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureInstances);
        }

        return numPlated;
    }

    private int AddPlatingForInventory(SaveDataWrapper saveDataWrapper)
    {
        var modifiedCount = 0;

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.PlayerInventory) is not
            { } playerInventory)
        {
            return modifiedCount;
        }

        if (playerInventory.SelectToken("EquippedItems") is JArray equippedItems)
        {
            modifiedCount += AddPlatings(equippedItems, true);
        }

        if (playerInventory.SelectToken("ItemInstanceManagerData.ItemBlocks") is JArray inventoryItems)
        {
            modifiedCount += AddPlatings(inventoryItems, false);
        }

        if (modifiedCount > 0)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.PlayerInventory);
        }

        return modifiedCount;
    }

    private static SaveDataWrapper? GetPlayerInventorySaveDataWrapper()
    {
        return SavegameManager.SelectedSavegame!.SavegameStore.LoadJsonRaw(SavegameStore.FileType
            .PlayerInventorySaveData);
    }

    private static SaveDataWrapper? GetScrewStructuresSaveDataWrapper()
    {
        return SavegameManager.SelectedSavegame!.SavegameStore.LoadJsonRaw(SavegameStore.FileType
            .ScrewStructureInstancesSaveData);
    }

    private static SaveDataWrapper? GetConstructionsSaveDataWrapper()
    {
        return SavegameManager.SelectedSavegame!.SavegameStore.LoadJsonRaw(SavegameStore.FileType
            .ConstructionsSaveData);
    }

    private static SaveDataWrapper? GetVailSaveDataWrapper()
    {
        return SavegameManager.SelectedSavegame!.SavegameStore.LoadJsonRaw(SavegameStore.FileType
            .SaveData);
    }

    private int AddPlatings(JArray items, bool isEquippedItems)
    {
        var numPlated = 0;
        foreach (var token in items)
        {
            if (token["ItemId"]?.Value<int>() is not { } itemId || !_platableItems.ContainsKey(itemId))
            {
                continue;
            }

            if (FindPlatingModules(token) is { } platingModules)
            {
                foreach (var platingModule in platingModules)
                {
                    var isPlated = platingModule["IsPlated"]?.Value<bool>() ?? false;
                    if (isPlated)
                    {
                        continue;
                    }

                    platingModule["IsPlated"] = true;
                    numPlated++;
                }
            }
            else
            {
                if (isEquippedItems)
                {
                    AddPlatingModuleToEquippedItem(token);
                }
                else
                {
                    AddPlatingModuleToInventoryItem(token);
                }

                numPlated++;
            }
        }

        return numPlated;
    }

    private static void AddPlatingModuleToEquippedItem(JToken token)
    {
        if (token["Modules"] is not JArray)
        {
            token["Modules"] = new JArray
            {
                CreatePlatingModule()
            };
        }
    }

    private static void AddPlatingModuleToInventoryItem(JToken token)
    {
        if (token["UniqueItems"] is not JArray uniqueItems)
        {
            uniqueItems = new JArray();
            token["UniqueItems"] = uniqueItems;
        }

        uniqueItems.Add(new JObject
        {
            ["Modules"] = new JArray
            {
                CreatePlatingModule()
            }
        });
    }

    private static JObject CreatePlatingModule()
    {
        return new JObject
        {
            ["IsPlated"] = true,
            ["Version"] = "0.0.0",
            ["ModuleId"] = 8
        };
    }

    private static List<JObject> FindPlatingModules(JToken token)
    {
        var foundModules = new List<JObject>();

        if (token["UniqueItems"] is JArray uniqueItems)
        {
            foreach (var uniqueItem in uniqueItems)
            {
                if (uniqueItem is not JObject jObject || jObject["Modules"] is not JArray modules)
                {
                    continue;
                }

                foundModules.AddRange(FindPlatingModules(modules));
            }
        }

        if (token["Modules"] is JArray otherModules)
        {
            foundModules.AddRange(FindPlatingModules(otherModules));
        }

        return foundModules;
    }

    private static IEnumerable<JObject> FindPlatingModules(JArray modules)
    {
        return modules.Where(module => module is JObject moduleData && moduleData["ModuleId"]?.Value<int>() == 8)
            .Cast<JObject>()
            .ToList();
    }

    [RelayCommand(CanExecute = nameof(CanModifyPlating))]
    private void RemovePlating()
    {
        var modifiedCount = 0;

        var wrappersToSave = new Dictionary<SavegameStore.FileType, SaveDataWrapper>();

        if (ItemsInInventory && GetPlayerInventorySaveDataWrapper() is { } playerInventorySaveDataWrapper)
        {
            var inventoryPlatingsRemoved = RemovePlatingFromInventoryItems(playerInventorySaveDataWrapper);
            if (inventoryPlatingsRemoved > 0)
            {
                wrappersToSave.Add(SavegameStore.FileType.PlayerInventorySaveData, playerInventorySaveDataWrapper);
            }

            modifiedCount += inventoryPlatingsRemoved;
        }

        if (ItemsInWeaponRacks && GetScrewStructuresSaveDataWrapper() is { } screwStructuresSaveDataWrapper)
        {
            var weaponRackPlatingsRemoved = RemovePlatingFromWeaponRacks(screwStructuresSaveDataWrapper);
            if (weaponRackPlatingsRemoved > 0)
            {
                wrappersToSave.Add(SavegameStore.FileType.ScrewStructureInstancesSaveData,
                    screwStructuresSaveDataWrapper);
            }

            modifiedCount += weaponRackPlatingsRemoved;
        }

        if (FollowerItems && GetVailSaveDataWrapper() is { } vailSaveDataWrapper)
        {
            var followerPlatingsRemoved = RemovePlatingsFromFollowers(vailSaveDataWrapper);
            if (followerPlatingsRemoved > 0)
            {
                wrappersToSave.Add(SavegameStore.FileType.SaveData, vailSaveDataWrapper);
            }

            modifiedCount += followerPlatingsRemoved;
        }

        if ((StorageStructures || FurnitureStructures || TrapsStructures || UtilityStructures || MiscStructures ||
             GardeningStructures) &&
            GetScrewStructuresSaveDataWrapper() is { } screwStructuresSaveDataWrapper2)
        {
            var structurePlatingsRemoved = RemovePlatingFromStructures(screwStructuresSaveDataWrapper2);
            if (structurePlatingsRemoved > 0)
            {
                wrappersToSave.TryAdd(SavegameStore.FileType.ScrewStructureInstancesSaveData,
                    screwStructuresSaveDataWrapper2);
            }

            modifiedCount += structurePlatingsRemoved;
        }

        if ((LogConstruction || LogPlankConstruction || StickConstruction || StoneConstruction || ItemConstruction) &&
            GetConstructionsSaveDataWrapper() is { } constructionsSaveDataWrapper)
        {
            var constructionPlatingsRemoved = RemovePlatingFromConstructions(constructionsSaveDataWrapper);
            if (constructionPlatingsRemoved > 0)
            {
                wrappersToSave.TryAdd(SavegameStore.FileType.ConstructionsSaveData,
                    constructionsSaveDataWrapper);
            }

            modifiedCount += constructionPlatingsRemoved;
        }

        if (wrappersToSave.Count > 0)
        {
            var selectedSavegame = SavegameManager.SelectedSavegame!;
            WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(selectedSavegame, backupMode =>
            {
                selectedSavegame.SavegameStore.SaveWrappers(backupMode, wrappersToSave);

                ShowSuccessMessage(modifiedCount, "windows.itemPlater.messages.removePlating");
            }));
        }
        else
        {
            ShowSuccessMessage(modifiedCount, "windows.itemPlater.messages.removePlating");
        }

        _parent.Close();
    }

    private int RemovePlatingFromConstructions(SaveDataWrapper saveDataWrapper)
    {
        var modifiedCount = 0;

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.Constructions) is not
                { } constructions || constructions["Structures"] is not JArray structures)
        {
            return modifiedCount;
        }

        var selectedProfiles = GetSelectedCategoryProfileIds(true);

        foreach (var structure in structures)
        {
            if (structure is not JArray strucArray)
            {
                continue;
            }

            foreach (var strucBlock in strucArray)
            {
                if (strucBlock["Elements"] is not JArray elements)
                {
                    continue;
                }

                foreach (var element in elements)
                {
                    if (element["ProfileID"]?.Value<int>() is not { } profileId ||
                        !selectedProfiles.TryGetValue(profileId, out var profile))
                    {
                        continue;
                    }

                    modifiedCount++;
                    element["ProfileID"] = profile.Id;
                }
            }
        }

        if (modifiedCount > 0)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.Constructions);
        }

        return modifiedCount;
    }

    private int RemovePlatingFromStructures(SaveDataWrapper saveDataWrapper)
    {
        var modifiedCount = 0;

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances) is not
                { } screwStructureInstances || screwStructureInstances["_structures"] is not JArray structures)
        {
            return modifiedCount;
        }

        var selectedStructureTypeIds = GetSelectedStructureTypeIds();

        foreach (var structure in structures)
        {
            if (structure["Id"]?.Value<int>() is not { } id || !selectedStructureTypeIds.Contains(id))
            {
                continue;
            }

            if (structure["Modules"] is not JArray modules)
            {
                continue;
            }

            structure["Modules"] = RemoveStructurePlatingModule(modules, out var countRemoved);

            modifiedCount += countRemoved;
        }

        if (modifiedCount > 0)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureInstances);
        }

        return modifiedCount;
    }

    private static JToken RemoveStructurePlatingModule(JArray modules, out int countRemoved)
    {
        countRemoved = 0;

        if (modules.Count == 0)
        {
            return modules;
        }

        var newArray = new JArray();

        foreach (var module in modules)
        {
            if (module["StateDataTypeId"]?.Value<int>() == 2)
            {
                countRemoved++;
                continue;
            }

            newArray.Add(module);
        }

        return newArray;
    }

    private int RemovePlatingsFromFollowers(SaveDataWrapper saveDataWrapper)
    {
        var modifiedCount = 0;

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.NpcItemInstances) is not
                { } npcItemInstances || npcItemInstances["ActorItems"] is not JArray actorItems)
        {
            return modifiedCount;
        }

        foreach (var actorData in actorItems)
        {
            if (actorData["Items"] is not JObject items || items["ItemBlocks"] is not JArray itemBlocks)
            {
                continue;
            }

            modifiedCount += AddPlatings(itemBlocks, false);
        }

        if (modifiedCount > 0)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.NpcItemInstances);
        }

        return modifiedCount;
    }

    private int RemovePlatingFromWeaponRacks(SaveDataWrapper saveDataWrapper)
    {
        var modifiedCount = 0;

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances) is not
                { } screwStructureInstances || screwStructureInstances["_structures"] is not JArray structures)
        {
            return modifiedCount;
        }

        var weaponRacks = from structure in structures
            let structureId = structure["Id"]?.Value<int>()
            where structureId.HasValue && _weaponHolderScrewStructureIds.Contains(structureId.Value)
            select structure;

        foreach (var weaponRack in weaponRacks)
        {
            if (weaponRack["Storages"] is not JArray storages)
            {
                continue;
            }

            foreach (var storage in storages)
            {
                if (storage["ItemBlocks"] is not JArray itemBlocks)
                {
                    continue;
                }

                modifiedCount += RemovePlatings(itemBlocks);
            }
        }

        if (modifiedCount > 0)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureInstances);
        }

        return modifiedCount;
    }

    private int RemovePlatingFromInventoryItems(SaveDataWrapper saveDataWrapper)
    {
        var modifiedCount = 0;
        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.PlayerInventory) is not { } playerInventory)
        {
            return modifiedCount;
        }

        if (playerInventory.SelectToken("EquippedItems") is JArray equippedItems)
        {
            modifiedCount += RemovePlatings(equippedItems);
        }

        if (playerInventory.SelectToken("ItemInstanceManagerData.ItemBlocks") is JArray inventoryItems)
        {
            modifiedCount += RemovePlatings(inventoryItems);
        }

        if (modifiedCount > 0)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.PlayerInventory);
        }

        return modifiedCount;
    }

    private int RemovePlatings(JArray items)
    {
        var numPlatingRemoved = 0;
        foreach (var token in items)
        {
            if (token["ItemId"]?.Value<int>() is not { } itemId || !_platableItems.ContainsKey(itemId))
            {
                continue;
            }

            foreach (var platingModule in FindPlatingModules(token))
            {
                var isPlated = platingModule["IsPlated"]?.Value<bool>() ?? false;
                if (!isPlated)
                {
                    continue;
                }

                platingModule["IsPlated"] = false;
                numPlatingRemoved++;
            }
        }

        return numPlatingRemoved;
    }
}