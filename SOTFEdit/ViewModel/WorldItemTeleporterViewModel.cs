using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;
using SOTFEdit.Model.WorldItem;

namespace SOTFEdit.ViewModel;

public partial class WorldItemTeleporterViewModel : ObservableObject
{
    private static readonly Dictionary<int, WorldItemType> ItemIdsToWorldItemTypes = new()
    {
        { 626, WorldItemType.HangGlider },
        { 630, WorldItemType.KnightV },
        { 590, WorldItemType.Radio }
    };

    private static readonly Dictionary<string, WorldItemType> ObjectNameIdToItemTypes = new()
    {
        { "GolfCart", WorldItemType.GolfCart },
        { "HangGlider", WorldItemType.HangGlider },
        { "KnightV", WorldItemType.KnightV }
    };

    private readonly ICloseableWithResult _parent;

    private readonly Savegame _savegame;

    [ObservableProperty]
    private WorldItemType? _selectedWorldItemType;

    public WorldItemTeleporterViewModel(Savegame savegame, ICloseableWithResult parent)
    {
        _savegame = savegame;
        _parent = parent;
    }

    partial void OnSelectedWorldItemTypeChanged(WorldItemType? value)
    {
        CloneObjectAtPlayerPosCommand.NotifyCanExecuteChanged();
        RemoveAllOfThisTypeCommand.NotifyCanExecuteChanged();
    }

    public static IEnumerable<WorldItemState> Load(Savegame savegame, bool includeRuntimeCreated)
    {
        var result = new List<WorldItemState>();

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WorldItemManagerSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.WorldItemManager) is not { } worldItemManager ||
            worldItemManager["WorldItemStates"] is not JArray worldItemStates)
        {
            return result;
        }

        var worldItemTypeCounts = new Dictionary<WorldItemType, int>();

        foreach (var worldItemState in worldItemStates)
        {
            var isRuntimeCreated = IsRuntimeCreated(worldItemState);

            if ((isRuntimeCreated && !includeRuntimeCreated) || worldItemState["Position"]?.ToObject<Position>() is not
                    { } position)
            {
                continue;
            }

            var itemId = worldItemState["ItemId"]?.Value<int>();
            var objectNameId = worldItemState["ObjectNameId"]?.Value<string>();
            var worldItemType = GetWorldItemType(itemId, objectNameId);
            var group = worldItemType == WorldItemType.Unknown
                ? TranslationManager.GetFormatted("windows.worldItemCloner.unknownItem",
                    objectNameId ?? (itemId?.ToString() ?? ""))
                : TranslationManager.Get("worldItemTypes." + worldItemType);

            worldItemTypeCounts[worldItemType] = worldItemTypeCounts.GetOrCreate(worldItemType) + 1;
            var objectName = $"{group} ({worldItemTypeCounts[worldItemType]})";

            result.Add(new WorldItemState(objectName, group, position, worldItemType, isRuntimeCreated));
        }

        return result;
    }

    private static bool IsRuntimeCreated(JToken worldItemState)
    {
        return (worldItemState["RuntimeCreated"]?.Value<bool>() ?? false) ||
               (worldItemState["Unnamed"]?.Value<bool>() ??
                false); //only for legacy reasons, has been replaced by RuntimeCreated
    }

    private static WorldItemType GetWorldItemType(int? itemId, string? objectNameId)
    {
        if (itemId is { } id && ItemIdsToWorldItemTypes.TryGetValue(id, out var itemTypeByItemId))
        {
            return itemTypeByItemId;
        }

        if (string.IsNullOrWhiteSpace(objectNameId))
        {
            return WorldItemType.Unknown;
        }

        var objectNameParts = objectNameId.Split('.');
        if (objectNameParts.Length >= 2 &&
            ObjectNameIdToItemTypes.TryGetValue(objectNameParts[1], out var itemTybeByObjectNameId))
        {
            return itemTybeByObjectNameId;
        }

        return WorldItemType.Unknown;
    }

    [RelayCommand(CanExecute = nameof(HasModifiableWorldItemTypeSelected))]
    private void RemoveAllOfThisType()
    {
        if (SelectedWorldItemType is not { } selectedWorldItemType)
        {
            return;
        }

        if (_savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WorldItemManagerSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.WorldItemManager) is not { } worldItemManager ||
            worldItemManager["WorldItemStates"] is not JArray worldItemStates)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.Get("windows.worldItemCloner.messages.nothingToDelete.text"),
                TranslationManager.Get("windows.worldItemCloner.messages.nothingToDelete.title")
            ));
            _parent.Close(false);
            return;
        }

        var toRemove = worldItemStates
            .Where(worldItemState => WorldItemHasType(worldItemState, selectedWorldItemType))
            .Where(IsRuntimeCreated)
            .ToList();

        if (toRemove.Count == 0)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.Get("windows.worldItemCloner.messages.nothingToDelete.text"),
                TranslationManager.Get("windows.worldItemCloner.messages.nothingToDelete.title")
            ));
            _parent.Close(false);
            return;
        }

        toRemove.ForEach(worldItemState => worldItemState.Remove());
        saveDataWrapper.MarkAsModified(Constants.JsonKeys.WorldItemManager);

        WeakReferenceMessenger.Default.Send(
            new GenericMessageEvent(
                TranslationManager.GetFormatted("windows.worldItemCloner.messages.clonesDeleted.text",
                    toRemove.Count, TranslationManager.Get("worldItemTypes." + selectedWorldItemType)),
                TranslationManager.Get("windows.worldItemCloner.messages.clonesDeleted.title")));

        _parent.Close(true);
    }

    private static bool WorldItemHasType(JToken worldItemState, WorldItemType requestedType)
    {
        var itemId = worldItemState["ItemId"]?.Value<int>();
        var objectNameId = worldItemState["ObjectNameId"]?.Value<string>();
        var worldItemType = GetWorldItemType(itemId, objectNameId);

        return worldItemType == requestedType;
    }

    [RelayCommand(CanExecute = nameof(HasModifiableWorldItemTypeSelected))]
    private void CloneObjectAtPlayerPos()
    {
        if (SelectedWorldItemType is not { } selectedWorldItemType ||
            CreateClone(selectedWorldItemType) is not { } clonedWorldItem)
        {
            return;
        }

        if (_savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WorldItemManagerSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.WorldItemManager) is not { } worldItemManager ||
            worldItemManager["WorldItemStates"] is not JArray worldItemStates)
        {
            WeakReferenceMessenger.Default.Send(
                new GenericMessageEvent(
                    TranslationManager.Get("windows.worldItemCloner.messages.nothingToMove.text"),
                    TranslationManager.Get("windows.worldItemCloner.messages.nothingToMove.title")));
            _parent.Close(false);
            return;
        }

        worldItemStates.Add(clonedWorldItem);
        saveDataWrapper.MarkAsModified(Constants.JsonKeys.WorldItemManager);

        var itemName = TranslationManager.Get("worldItemTypes." + selectedWorldItemType);

        WeakReferenceMessenger.Default.Send(
            new GenericMessageEvent(
                TranslationManager.GetFormatted("windows.worldItemCloner.messages.objectCloned.text", itemName),
                TranslationManager.GetFormatted("windows.worldItemCloner.messages.objectCloned.title", itemName)));

        _parent.Close(true);
    }

    private static JObject? CreateClone(WorldItemType? selectedWorldItemType)
    {
        if (selectedWorldItemType is not { } worldItemType || !CanClone(worldItemType))
        {
            return null;
        }

        var itemId = ItemIdsToWorldItemTypes.Where(kvp => kvp.Value == worldItemType)
            .Select(kvp => kvp.Key)
            .FirstOrDefault(-1);

        if (itemId == -1)
        {
            return null;
        }

        var playerPos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos;

        return new JObject
        {
            { "ObjectNameId", GetObjectNameId(worldItemType) },
            { "ItemId", itemId },
            { "Position", JToken.FromObject(playerPos) },
            {
                "Rotation", new JObject
                {
                    { "x", 0f },
                    { "y", 0f },
                    { "z", 0f }
                }
            },
            { "RuntimeCreated", true }
        };
    }

    private static string GetObjectNameId(WorldItemType worldItemType)
    {
        return worldItemType switch
        {
            WorldItemType.Radio => "",
            _ => Guid.NewGuid().ToString()
        };
    }

    private bool HasWorldItemTypeSelected()
    {
        return SelectedWorldItemType != null;
    }

    private bool HasModifiableWorldItemTypeSelected()
    {
        return HasWorldItemTypeSelected() && CanClone(SelectedWorldItemType);
    }

    private static bool CanClone(WorldItemType? worldItemType)
    {
        return worldItemType switch
        {
            WorldItemType.HangGlider => true,
            WorldItemType.KnightV => true,
            WorldItemType.Radio => true,
            _ => false
        };
    }
}