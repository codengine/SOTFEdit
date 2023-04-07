using System.Collections.Generic;
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
    private readonly ICloseable _parent;

    [ObservableProperty] private WorldItemState? _selectedWorldItem;

    public WorldItemTeleporterViewModel(GameData gameData, Savegame savegame, ICloseable parent)
    {
        _parent = parent;
        WorldItemStates = new ListCollectionView(Load(gameData.Items, savegame).OrderBy(state => state.Group)
            .ThenBy(state => state.ObjectNameId).ToList())
        {
            GroupDescriptions =
            {
                new PropertyGroupDescription("Group")
            }
        };
    }

    public ListCollectionView WorldItemStates { get; }

    partial void OnSelectedWorldItemChanged(WorldItemState? value)
    {
        OpenMapAtObjectPosCommand.NotifyCanExecuteChanged();
        CloneObjectAtPlayerPosCommand.NotifyCanExecuteChanged();
        TeleportPlayerToObjectCommand.NotifyCanExecuteChanged();
        RemoveAllOfThisTypeCommand.NotifyCanExecuteChanged();
    }

    private static IEnumerable<WorldItemState> Load(ItemList items, Savegame savegame)
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
                string.IsNullOrEmpty(objectNameId) ||
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
                    groupName = $"Unknown Item ({itemId})";
                }
            }
            else
            {
                continue;
            }

            result.Add(new WorldItemState(itemId, objectNameId == "" ? $"Unnamed ({i++})" : objectNameId, groupName,
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
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent("Nothing to delete", "Nothing found"));
            _parent.Close();
            return;
        }

        var toRemove = worldItemStates
            .Where(worldItemState => worldItemState["ItemId"]?.Value<int>() == _selectedWorldItem?.ItemId)
            .Where(worldItemState => worldItemState["Unnamed"]?.Value<bool>() == true)
            .ToList();

        if (toRemove.Count == 0)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent("Nothing to delete", "Nothing found"));
            _parent.Close();
            return;
        }

        toRemove.ForEach(worldItemState => worldItemState.Remove());
        WeakReferenceMessenger.Default.Send(
            new GenericMessageEvent($"{toRemove.Count} clones of {_selectedWorldItem.Group} were removed",
                "Clones removed"));
        _parent.Close();
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
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent("Nothing to move found", "Nothing found"));
            _parent.Close();
            return;
        }

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
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                $"A copy of {_selectedWorldItem.ObjectNameId} was created at player position. Please save changes to persist them",
                $"{_selectedWorldItem.Group} created"));
            break;
        }

        _parent.Close();
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
        WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
            $"Player was moved to {_selectedWorldItem.ObjectNameId}. Please save changes to persist it",
            $"{_selectedWorldItem.Group} moved"));
        _parent.Close();
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