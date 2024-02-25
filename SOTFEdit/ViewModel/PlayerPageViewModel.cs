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
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public partial class PlayerPageViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly ObservableCollection<Item> _availableClothes = new();

    private readonly GameData _gameData;

    public PlayerPageViewModel(GameData gameData)
    {
        AvailableClothesView = CollectionViewSource.GetDefaultView(_availableClothes);
        AvailableClothesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

        _gameData = gameData;
        SetupListeners();
    }

    public PlayerState PlayerState { get; } = new();
    public ArmorPage ArmorPage { get; } = new();
    public ICollectionView AvailableClothesView { get; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => OnSelectedSavegameChanged(m));
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        if (message.SelectedSavegame is { } selectedSavegame)
        {
            LoadPlayerData(selectedSavegame);
            LoadClothes(selectedSavegame);
        }
        else
        {
            PlayerState.Reset();
        }

        MoveToKelvinCommand.NotifyCanExecuteChanged();
        MoveToVirginiaCommand.NotifyCanExecuteChanged();
        FillAllBarsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void OpenMapAtPlayerPos()
    {
        WeakReferenceMessenger.Default.Send(new ZoomToPosEvent(PlayerState.Pos));
    }

    private void LoadClothes(Savegame savegame)
    {
        _availableClothes.Clear();
        var newAvailableClothes = GetDefaultAvailableClothes()
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerClothingSystemSaveData)
                ?.GetJsonBasedToken(Constants.JsonKeys.PlayerClothingSystem) is not
            { } playerClothingSystem)
        {
            return;
        }

        if (playerClothingSystem["Clothing"] is JArray clothing)
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

        foreach (var kvp in newAvailableClothes)
        {
            _availableClothes.Add(kvp.Value);
        }

        PlayerState.SelectedCloth ??=
            _availableClothes.FirstOrDefault(cloth => cloth.Id == Constants.Items.DefaultPlayerClothItemId);
    }

    private IEnumerable<KeyValuePair<int, Item>> GetDefaultAvailableClothes()
    {
        return _gameData.Items.Where(item => item.Value.Type == "clothes" || item.Value.IsWearableCloth);
    }

    public static bool CanSaveChanges()
    {
        return SavegameManager.SelectedSavegame != null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToVirginia()
    {
        var followerState = Ioc.Default.GetRequiredService<FollowerPageViewModel>()
            .VirginiaState;
        MoveToFollower(followerState);
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToKelvin()
    {
        var followerState = Ioc.Default.GetRequiredService<FollowerPageViewModel>()
            .KelvinState;
        MoveToFollower(followerState);
    }

    private void MoveToFollower(FollowerState followerState)
    {
        var followerPos = followerState.Pos;
        var playerPos = PlayerState.Pos;

        Teleporter.MovePlayerToPos(ref playerPos, ref followerPos);

        followerState.Pos = followerPos;
        PlayerState.Pos = playerPos;
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void FillAllBars()
    {
        PlayerState.CurrentHealth = PlayerState.MaxHealth;
        PlayerState.Fullness = 100;
        PlayerState.Hydration = 100;
        PlayerState.Rest = 100;
        PlayerState.Stamina = 100;
        PlayerState.HydrationBuff = 65535;
        PlayerState.RestBuff = 1;
        PlayerState.FullnessBuff = 65535;
        PlayerState.Sickness = 0;
    }

    private void LoadPlayerData(Savegame savegame)
    {
        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerStateSaveData)
                    ?.GetJsonBasedToken(Constants.JsonKeys.PlayerState) is not JObject
                playerState ||
            playerState["_entries"] is not JArray entries)
        {
            return;
        }

        Position? playerPos = null;
        var areaMask = 0;

        foreach (var entry in entries)
        {
            var name = entry["Name"]?.ToString();

            switch (name)
            {
                case "player.position":
                    var playerPosFloatArray = entry["FloatArrayValue"]?.ToObject<float[]>();

                    if (playerPosFloatArray is { Length: 3 })
                    {
                        playerPos = new Position(playerPosFloatArray[0], playerPosFloatArray[1],
                            playerPosFloatArray[2]);
                    }

                    break;
                case "player.areaMask":
                    areaMask = SettingReader.ReadInt(entry) ?? 0;
                    break;
                case "StrengthLevel":
                    PlayerState.StrengthLevel = SettingReader.ReadInt(entry) ?? 0;
                    break;
                case "MaxHealth":
                    PlayerState.MaxHealth = SettingReader.ReadFloat(entry) ?? 0f;
                    break;
                case "CurrentHealth":
                    PlayerState.CurrentHealth = SettingReader.ReadFloat(entry) ?? 0f;
                    break;
                case "Hydration":
                    PlayerState.Hydration = SettingReader.ReadFloat(entry) ?? 0f;
                    break;
                case "HydrationBuff":
                    PlayerState.HydrationBuff = SettingReader.ReadFloat(entry) ?? 0f;
                    break;
                case "Fullness":
                    PlayerState.Fullness = SettingReader.ReadFloat(entry) ?? 0f;
                    break;
                case "FullnessBuff":
                    PlayerState.FullnessBuff = SettingReader.ReadFloat(entry) ?? 0f;
                    break;
                case "Rest":
                    PlayerState.Rest = SettingReader.ReadFloat(entry) ?? 0f;
                    break;
                case "RestBuff":
                    PlayerState.RestBuff = SettingReader.ReadFloat(entry) ?? 0f;
                    break;
                case "Stamina":
                    PlayerState.Stamina = SettingReader.ReadFloat(entry) ?? 0f;
                    break;
                case "Sickness":
                    PlayerState.Sickness = SettingReader.ReadFloat(entry) ?? 0f;
                    break;
            }
        }

        if (playerPos is { } pos)
        {
            PlayerState.Pos = pos.WithoutOffset(_gameData.AreaManager.GetAreaForAreaMask(areaMask));
        }
    }

    public bool Update(Savegame? savegame)
    {
        var hasChanges = UpdatePlayerState(savegame);
        hasChanges = UpdateClothes(savegame) || hasChanges;
        return hasChanges;
    }

    private bool UpdateClothes(Savegame? savegame)
    {
        if (savegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerClothingSystemSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.PlayerClothingSystem)?["Clothing"] is not JArray
                clothing)
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

        if (PlayerState.SelectedCloth is { } selectedCloth &&
            selectedCloth.Id != Constants.Items.DefaultPlayerClothItemId)
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

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.PlayerClothingSystem);

        return true;
    }

    private bool UpdatePlayerState(Savegame? savegame)
    {
        if (savegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.PlayerStateSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.PlayerState) is not JObject playerState ||
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
                case "player.areaMask":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.Pos.Area.AreaMask) || hasChanges;
                    break;
                case "StrengthLevel":
                    hasChanges = SettingWriter.WriteInt(entry, PlayerState.StrengthLevel) || hasChanges;
                    break;
                case "MaxHealth":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.MaxHealth) || hasChanges;
                    break;
                case "CurrentHealth":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.CurrentHealth) || hasChanges;
                    break;
                case "TargetHealth":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.CurrentHealth) || hasChanges;
                    break;
                case "Hydration":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.Hydration) || hasChanges;
                    break;
                case "HydrationBuff":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.HydrationBuff) || hasChanges;
                    break;
                case "Fullness":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.Fullness) || hasChanges;
                    break;
                case "FullnessBuff":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.FullnessBuff) || hasChanges;
                    break;
                case "Rest":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.Rest) || hasChanges;
                    break;
                case "RestBuff":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.RestBuff) || hasChanges;
                    break;
                case "Stamina":
                    hasChanges = SettingWriter.WriteFloat(entry, PlayerState.Stamina) || hasChanges;
                    break;
                case "Sickness":
                    if (PlayerState.Sickness == 0)
                    {
                        hasChanges = SettingWriter.RemoveFloat(entry) || hasChanges;
                    }
                    else
                    {
                        hasChanges = SettingWriter.WriteFloat(entry, PlayerState.Sickness) || hasChanges;
                    }

                    break;
            }
        }

        if (!hasChanges)
        {
            return false;
        }

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.PlayerState);

        return true;
    }
}