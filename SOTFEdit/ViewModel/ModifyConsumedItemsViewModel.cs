using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public partial class ModifyConsumedItemsViewModel
{
    private const string Prefix = "consumed.";
    private readonly ICloseable _parent;

    public ModifyConsumedItemsViewModel(Savegame savegame, GameData gameData,
        ICloseable parent)
    {
        _parent = parent;
        Load(savegame, gameData.Items);
    }

    public ObservableCollectionEx<ConsumedItemWrapper> ConsumedItems { get; } = new();

    private void Load(Savegame savegame, ItemList items)
    {
        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerStateSaveData) is not { } saveDataWrapper)
        {
            return;
        }

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.PlayerState) is not { } playerState ||
            playerState["_entries"] is not JArray entries)
        {
            return;
        }

        var consumedItems = new List<ConsumedItemWrapper>();

        foreach (var entry in entries)
        {
            if (entry["Name"]?.Value<string>() is not { } name || !name.StartsWith("consumed."))
            {
                continue;
            }

            var item = GetItemId(name) is { } itemId ? items.GetItem(itemId) : null;
            consumedItems.Add(new ConsumedItemWrapper(name, item));
        }

        ConsumedItems.ReplaceRange(consumedItems.OrderBy(wrapper => wrapper.Name));
    }

    [RelayCommand]
    private void RemoveAll()
    {
        if (SavegameManager.SelectedSavegame is not { } savegame)
        {
            return;
        }

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerStateSaveData) is not { } saveDataWrapper)
        {
            return;
        }

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.PlayerState) is not { } playerState ||
            playerState["_entries"] is not JArray entries)
        {
            return;
        }

        var toBeRemoved = ConsumedItems.Select(wrapper => wrapper.Key)
            .ToHashSet();
        var hasChanges = RemoveEntries(entries, toBeRemoved);
        if (hasChanges)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.PlayerState);
        }

        _parent.Close();
    }

    private static bool RemoveEntries(JArray entries, IReadOnlySet<string> toBeRemoved)
    {
        if (toBeRemoved.Count == 0)
        {
            return false;
        }

        var tokensToBeRemoved = new List<JToken>();

        foreach (var entry in entries)
        {
            if (entry["Name"]?.Value<string>() is not { } name || !toBeRemoved.Contains(name))
            {
                continue;
            }

            tokensToBeRemoved.Add(entry);
        }

        foreach (var tokenToBeRemoved in tokensToBeRemoved)
        {
            entries.Remove(tokenToBeRemoved);
        }

        return tokensToBeRemoved.Count > 0;
    }

    [RelayCommand]
    private void Save()
    {
        if (SavegameManager.SelectedSavegame is not { } savegame)
        {
            return;
        }

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerStateSaveData) is not { } saveDataWrapper)
        {
            return;
        }

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.PlayerState) is not { } playerState ||
            playerState["_entries"] is not JArray entries)
        {
            return;
        }

        var toBeRemoved = ConsumedItems.Where(wrapper => wrapper.Remove)
            .Select(wrapper => wrapper.Key)
            .ToHashSet();
        var hasChanges = RemoveEntries(entries, toBeRemoved);
        if (hasChanges)
        {
            saveDataWrapper.MarkAsModified(Constants.JsonKeys.PlayerState);
        }

        _parent.Close();
    }

    private static int? GetItemId(string key)
    {
        if (int.TryParse(key.Replace(Prefix, "").TrimStart('0'), out var result))
        {
            return result;
        }

        return null;
    }

    public partial class ConsumedItemWrapper : ObservableObject
    {
        private readonly Item? _item;

        [ObservableProperty]
        private bool _remove;

        public ConsumedItemWrapper(string key, Item? item)
        {
            _item = item;
            Key = key;
        }

        public string Key { get; }

        public string Name => _item != null ? _item.Name : Key;
    }
}