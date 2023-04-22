using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public partial class WorldItemTeleporterViewModel : ObservableObject
{
    private readonly ICloseableWithResult _parent;

    [ObservableProperty]
    private WorldItemState? _selectedWorldItem;

    public WorldItemTeleporterViewModel(GameData gameData, Savegame savegame, ICloseableWithResult parent)
    {
        _parent = parent;
        WorldItemStates = CollectionViewSource.GetDefaultView(Load(gameData.Items, savegame)
            .OrderBy(state => state.Group)
            .ThenBy(state => state.ObjectNameId).ToList());
        WorldItemStates.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
    }

    public ICollectionView WorldItemStates { get; }

    partial void OnSelectedWorldItemChanged(WorldItemState? value)
    {
        OpenMapAtObjectPosCommand.NotifyCanExecuteChanged();
        CloneObjectAtPlayerPosCommand.NotifyCanExecuteChanged();
        TeleportPlayerToObjectCommand.NotifyCanExecuteChanged();
        RemoveAllOfThisTypeCommand.NotifyCanExecuteChanged();
    }

    public static IEnumerable<WorldItemState> Load(ItemList items, Savegame savegame, bool includeUnnamed = false)
    {
        var result = new List<WorldItemState>();

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WorldItemManagerSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.WorldItemManager) is not { } worldItemManager ||
            worldItemManager["WorldItemStates"] is not JArray worldItemStates)
        {
            return result;
        }

        var i = 0;

        foreach (var worldItemState in worldItemStates)
        {
            if (worldItemState["ObjectNameId"]?.Value<string>() is not { } objectNameId ||
                (!includeUnnamed && string.IsNullOrEmpty(objectNameId)) ||
                worldItemState["Position"]?.ToObject<Position>() is not { } position)
            {
                continue;
            }

            string groupName;

            if (worldItemState["ItemId"]?.Value<int>() is { } itemId)
            {
                if (items.GetItem(itemId) is { } item)
                {
                    groupName = item.Name;
                }
                else
                {
                    groupName = TranslationManager.GetFormatted("windows.worldItemTeleporter.unknownItem", itemId);
                }
            }
            else
            {
                continue;
            }

            result.Add(new WorldItemState(itemId,
                objectNameId == ""
                    ? TranslationManager.GetFormatted("windows.worldItemTeleporter.unnamedItem", i++)
                    : objectNameId, groupName,
                position));
        }

        return result;
    }

    [RelayCommand(CanExecute = nameof(HasWorldItemSelected))]
    private void RemoveAllOfThisType()
    {
        if (_selectedWorldItem == null || SavegameManager.SelectedSavegame is not { } savegame)
        {
            return;
        }

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WorldItemManagerSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.WorldItemManager) is not { } worldItemManager ||
            worldItemManager["WorldItemStates"] is not JArray worldItemStates)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.Get("windows.worldItemTeleporter.messages.nothingToDelete.text"),
                TranslationManager.Get("windows.worldItemTeleporter.messages.nothingToDelete.title")
            ));
            _parent.Close(false);
            return;
        }

        var toRemove = worldItemStates
            .Where(worldItemState => worldItemState["ItemId"]?.Value<int>() == _selectedWorldItem?.ItemId)
            .Where(worldItemState => worldItemState["Unnamed"]?.Value<bool>() == true)
            .ToList();

        if (toRemove.Count == 0)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.Get("windows.worldItemTeleporter.messages.nothingToDelete.text"),
                TranslationManager.Get("windows.worldItemTeleporter.messages.nothingToDelete.title")
            ));
            _parent.Close(false);
            return;
        }

        toRemove.ForEach(worldItemState => worldItemState.Remove());
        WeakReferenceMessenger.Default.Send(
            new GenericMessageEvent(
                TranslationManager.GetFormatted("windows.worldItemTeleporter.messages.clonesDeleted.text",
                    toRemove.Count, _selectedWorldItem.Group),
                TranslationManager.Get("windows.worldItemTeleporter.messages.clonesDeleted.title")));
        _parent.Close(toRemove.Count > 0);
    }

    [RelayCommand(CanExecute = nameof(HasWorldItemSelected))]
    private void CloneObjectAtPlayerPos()
    {
        if (_selectedWorldItem == null || SavegameManager.SelectedSavegame is not { } savegame)
        {
            return;
        }

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WorldItemManagerSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.WorldItemManager) is not { } worldItemManager ||
            worldItemManager["WorldItemStates"] is not JArray worldItemStates)
        {
            WeakReferenceMessenger.Default.Send(
                new GenericMessageEvent(
                    TranslationManager.Get("windows.worldItemTeleporter.messages.nothingToMove.text"),
                    TranslationManager.Get("windows.worldItemTeleporter.messages.nothingToMove.title")));
            _parent.Close(false);
            return;
        }

        var hasChanges = false;

        foreach (var worldItemState in worldItemStates)
        {
            if (worldItemState["ObjectNameId"]?.Value<string>() is not { } objectNameId ||
                objectNameId != _selectedWorldItem.ObjectNameId)
            {
                continue;
            }

            var playerPos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos;
            var itemStateCopy = worldItemState.DeepClone();
            itemStateCopy["ObjectNameId"] = "";
            itemStateCopy["Position"] = JToken.FromObject(playerPos);
            itemStateCopy["State"]?.Remove();
            itemStateCopy["Unnamed"] = true;
            worldItemStates.Add(itemStateCopy);
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.WorldItemManager);
            hasChanges = true;

            WeakReferenceMessenger.Default.Send(
                new GenericMessageEvent(
                    TranslationManager.GetFormatted("windows.worldItemTeleporter.messages.objectCloned.text",
                        _selectedWorldItem.ObjectNameId),
                    TranslationManager.GetFormatted("windows.worldItemTeleporter.messages.objectCloned.title",
                        _selectedWorldItem.Group)));
            break;
        }

        _parent.Close(hasChanges);
    }

    [RelayCommand(CanExecute = nameof(HasWorldItemSelected))]
    private void TeleportPlayerToObject()
    {
        if (_selectedWorldItem == null)
        {
            return;
        }

        Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos =
            _selectedWorldItem.Position.WithYOffset(3);

        WeakReferenceMessenger.Default.Send(
            new GenericMessageEvent(
                TranslationManager.GetFormatted("windows.worldItemTeleporter.messages.playerMoved.text",
                    _selectedWorldItem.ObjectNameId),
                TranslationManager.GetFormatted("windows.worldItemTeleporter.messages.playerMoved.title",
                    _selectedWorldItem.Group)));
        _parent.Close(false);
    }

    [RelayCommand(CanExecute = nameof(HasWorldItemSelected))]
    private void OpenMapAtObjectPos()
    {
        if (_selectedWorldItem is { } selected)
        {
            WeakReferenceMessenger.Default.Send(
                new ZoomToPosEvent(selected.Position));
        }
    }

    private bool HasWorldItemSelected()
    {
        return SelectedWorldItem != null;
    }
}