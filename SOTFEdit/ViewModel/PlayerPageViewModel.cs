using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public partial class PlayerPageViewModel : ObservableObject
{
    private const int TeleportYoffset = 1;
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly ObservableCollection<Item> _availableClothes = new();

    private readonly GameData _gameData;

    [NotifyCanExecuteChangedFor(nameof(MoveToKelvinCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToVirginiaCommand))]
    [NotifyCanExecuteChangedFor(nameof(FillAllBarsCommand))]
    [ObservableProperty]
    private Savegame? _selectedSavegame;

    public PlayerPageViewModel(GameData gameData)
    {
        AvailableClothesView = new GenericCollectionView<Item>(
            (ListCollectionView)CollectionViewSource.GetDefaultView(_availableClothes))
        {
            SortDescriptions =
            {
                new SortDescription("Name", ListSortDirection.Ascending)
            }
        };

        _gameData = gameData;
        SetupListeners();
    }

    public PlayerState PlayerState { get; } = new();
    public ArmorPage ArmorPage { get; } = new();
    public ICollectionView<Item> AvailableClothesView { get; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        SelectedSavegame = message.SelectedSavegame;
        if (SelectedSavegame is { } selectedSavegame)
        {
            LoadPlayerData(selectedSavegame);
            LoadClothes(selectedSavegame);
        }
        else
        {
            PlayerState.Reset();
        }
    }

    private void LoadClothes(Savegame savegame)
    {
        _availableClothes.Clear();
        var newAvailableClothes = GetDefaultAvailableClothes()
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var playerClothingSystemSaveData = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerClothingSystemSaveData);
        if (playerClothingSystemSaveData == null)
        {
            return;
        }

        var saveDataWrapper = new SaveDataWrapper(playerClothingSystemSaveData);
        var playerClothingSystemToken = saveDataWrapper.GetJsonBasedToken("Data.PlayerClothingSystem");

        if (playerClothingSystemToken?["Clothing"] is JArray clothing)
        {
            foreach (var itemId in clothing.Select(token => token.Value<int>()))
            {
                var item = _gameData.Items.GetItem(itemId);
                if (item == null)
                {
                    Logger.Info($"No item found for itemId {itemId}");
                }
                else
                {
                    var itemInAvailableClothes = newAvailableClothes.GetValueOrDefault(itemId);
                    if (itemInAvailableClothes != null)
                    {
                        PlayerState.SelectedCloth = itemInAvailableClothes;
                    }
                    else
                    {
                        newAvailableClothes.Add(itemId, item);
                    }
                }
            }
        }

        _availableClothes.Add(new Item
        {
            Id = -1,
            Name = ""
        });
        foreach (var kvp in newAvailableClothes) _availableClothes.Add(kvp.Value);
    }

    private IEnumerable<KeyValuePair<int, Item>> GetDefaultAvailableClothes()
    {
        return _gameData.Items.Where(item => item.Value.Type == "clothes" || item.Value.IsWearableCloth);
    }

    public bool CanSaveChanges()
    {
        return SelectedSavegame != null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToVirginia()
    {
        var virginiaPos = Ioc.Default.GetRequiredService<FollowerPageViewModel>()
            .VirginiaState.Pos;
        PlayerState.Pos = virginiaPos with { Y = virginiaPos.Y + TeleportYoffset };
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToKelvin()
    {
        var kelvinPos = Ioc.Default.GetRequiredService<FollowerPageViewModel>()
            .KelvinState.Pos;
        PlayerState.Pos = kelvinPos with { Y = kelvinPos.Y + TeleportYoffset };
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void FillAllBars()
    {
        PlayerState.CurrentHealth = PlayerState.MaxHealth;
        PlayerState.Fullness = 100;
        PlayerState.Hydration = 100;
        PlayerState.Rest = 100;
        PlayerState.Stamina = 100;
    }

    private void LoadPlayerData(Savegame savegame)
    {
        var playerStateSaveData = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerStateSaveData);
        var playerStateToken = playerStateSaveData?.SelectToken("Data.PlayerState");
        if (playerStateToken?.ToString() is not { } playerStateJson ||
            JsonConverter.DeserializeRaw(playerStateJson) is not JObject playerState ||
            playerState["_entries"] is not JArray entries)
        {
            return;
        }

        foreach (var entry in entries)
        {
            var name = entry["Name"]?.ToString();

            switch (name)
            {
                case "player.position":
                    var playerPosFloatArray = entry["FloatArrayValue"]?.ToObject<float[]>();

                    if (playerPosFloatArray is { Length: 3 })
                    {
                        PlayerState.Pos = new Position(playerPosFloatArray[0], playerPosFloatArray[1],
                            playerPosFloatArray[2]);
                    }

                    break;
                case "StrengthLevel":
                    PlayerState.StrengthLevel = ReadInt(entry) ?? 0;
                    break;
                case "MaxHealth":
                    PlayerState.MaxHealth = ReadFloat(entry) ?? 0f;
                    break;
                case "CurrentHealth":
                    PlayerState.CurrentHealth = ReadFloat(entry) ?? 0f;
                    break;
                case "Hydration":
                    PlayerState.Hydration = ReadFloat(entry) ?? 0f;
                    break;
                case "Fullness":
                    PlayerState.Fullness = ReadFloat(entry) ?? 0f;
                    break;
                case "Rest":
                    PlayerState.Rest = ReadFloat(entry) ?? 0f;
                    break;
                case "Stamina":
                    PlayerState.Stamina = ReadFloat(entry) ?? 0f;
                    break;
            }
        }
    }

    public bool Update(Savegame? savegame, bool createBackup)
    {
        var hasChanges = UpdatePlayerState(savegame, createBackup);
        hasChanges = UpdateClothes(savegame, createBackup) || hasChanges;
        return hasChanges;
    }

    private bool UpdateClothes(Savegame? savegame, bool createBackup)
    {
        var playerClothingSystemSaveData = savegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerClothingSystemSaveData);
        if (savegame == null || playerClothingSystemSaveData == null)
        {
            return false;
        }

        var saveDataWrapper = new SaveDataWrapper(playerClothingSystemSaveData);
        var playerClothingSystemToken = saveDataWrapper.GetJsonBasedToken("Data.PlayerClothingSystem");
        if (playerClothingSystemToken?["Clothing"] is not JArray clothing)
        {
            return false;
        }

        var newClothings = clothing.ToList();
        if (newClothings.Count > 1)
        {
            var oldClothingIds = newClothings.Select(token => token.Value<int>());
            Logger.Warn($"More than one cloth found in clothings ({oldClothingIds}) - will not update");
            return false;
        }

        var existingClothing = newClothings.FirstOrDefault();

        if (PlayerState.SelectedCloth is { } selectedCloth && selectedCloth.Id != -1)
        {
            if (existingClothing?.Value<int>() is { } existingItemId)
            {
                if (existingItemId == selectedCloth.Id)
                {
                    return false;
                }
            }

            clothing.ReplaceAll(new JValue(PlayerState.SelectedCloth.Id));
        }
        else
        {
            if (clothing.Count == 0)
            {
                return false;
            }

            clothing.Clear();
        }

        saveDataWrapper.MarkAsModified("Data.PlayerClothingSystem");
        saveDataWrapper.SerializeAllModified();

        savegame.SavegameStore.StoreJson(SavegameStore.FileType.PlayerClothingSystemSaveData, playerClothingSystemSaveData, createBackup);

        return true;
    }

    private bool UpdatePlayerState(Savegame? savegame, bool createBackup)
    {
        if (savegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerStateSaveData) is not JObject
            playerStateSaveData)
        {
            return false;
        }

        var playerStateToken = playerStateSaveData.SelectToken("Data.PlayerState");
        if (playerStateToken?.ToString() is not { } playerStateJson ||
            JsonConverter.DeserializeRaw(playerStateJson) is not JObject playerState ||
            playerState["_entries"] is not JArray entries)
        {
            return false;
        }

        var hasChanges = false;

        foreach (var entry in entries)
        {
            var name = entry["Name"]?.ToString();

            switch (name)
            {
                case "player.position":
                    if (entry["FloatArrayValue"] is { } floatArrayToken &&
                        floatArrayToken.ToObject<float[]>() is { Length: 3 } playerPosFloatArray)
                    {
                        var oldPlayerPos = new Position(playerPosFloatArray[0], playerPosFloatArray[1],
                            playerPosFloatArray[2]);
                        if (!oldPlayerPos.Equals(PlayerState.Pos))
                        {
                            floatArrayToken.Replace(JToken.FromObject(new[]
                                { PlayerState.Pos.X, PlayerState.Pos.Y, PlayerState.Pos.Z }));
                            hasChanges = true;
                        }
                    }

                    break;
                case "StrengthLevel":
                    hasChanges = WriteInt(entry, PlayerState.StrengthLevel) || hasChanges;
                    break;
                case "MaxHealth":
                    hasChanges = WriteFloat(entry, PlayerState.MaxHealth) || hasChanges;
                    break;
                case "CurrentHealth":
                    hasChanges = WriteFloat(entry, PlayerState.CurrentHealth) || hasChanges;
                    break;
                case "Hydration":
                    hasChanges = WriteFloat(entry, PlayerState.Hydration) || hasChanges;
                    break;
                case "Fullness":
                    hasChanges = WriteFloat(entry, PlayerState.Fullness) || hasChanges;
                    break;
                case "Rest":
                    hasChanges = WriteFloat(entry, PlayerState.Rest) || hasChanges;
                    break;
                case "Stamina":
                    hasChanges = WriteFloat(entry, PlayerState.Stamina) || hasChanges;
                    break;
            }
        }

        if (!hasChanges)
        {
            return false;
        }

        playerStateToken.Replace(JsonConverter.Serialize(playerState));

        savegame.SavegameStore.StoreJson(SavegameStore.FileType.PlayerStateSaveData, playerStateSaveData, createBackup);
        return true;
    }

    private static bool WriteFloat(JToken? target, float newValue)
    {
        if (target?["FloatValue"] is not { } targetToken || ReadFloat(target) is not { } oldValue ||
            Math.Abs(oldValue - newValue) < 0.001)
        {
            return false;
        }

        targetToken.Replace(newValue);
        return true;
    }

    private static bool WriteInt(JToken? target, int newValue)
    {
        if (target?["IntValue"] is not { } targetToken || ReadInt(target) is not { } oldValue || oldValue == newValue)
        {
            return false;
        }

        targetToken.Replace(newValue);
        return true;
    }

    private static float? ReadFloat(JToken? token)
    {
        return token?["FloatValue"]?.Value<float>();
    }

    private static int? ReadInt(JToken? token)
    {
        return token?["IntValue"]?.Value<int>();
    }
}