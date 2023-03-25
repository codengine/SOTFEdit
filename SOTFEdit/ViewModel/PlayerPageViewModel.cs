using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public partial class PlayerPageViewModel : ObservableObject
{
    [NotifyCanExecuteChangedFor(nameof(MoveToKelvinCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToVirginiaCommand))]
    [NotifyCanExecuteChangedFor(nameof(FillAllBarsCommand))]
    [ObservableProperty]
    private Savegame? _selectedSavegame;

    public PlayerPageViewModel()
    {
        SetupListeners();
    }

    public PlayerState PlayerState { get; } = new();
    public ArmorPage ArmorPage { get; } = new();

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
        }
        else
        {
            PlayerState.Reset();
        }
    }

    public bool CanSaveChanges()
    {
        return SelectedSavegame != null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToVirginia()
    {
        PlayerState.Pos = Ioc.Default.GetRequiredService<FollowerPageViewModel>()
            .VirginiaState.Pos.Copy();
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToKelvin()
    {
        PlayerState.Pos = Ioc.Default.GetRequiredService<FollowerPageViewModel>()
            .KelvinState.Pos.Copy();
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