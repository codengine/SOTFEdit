using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Settings;
using SOTFEdit.Model.SaveData.Setup;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public class GameSetupPageViewModel : ObservableObject
{
    private readonly Dictionary<string, GameSettingLightModel> _gameSettings = new();

    public GameSetupPageViewModel()
    {
        SetupListeners();
    }

    public string? SelectedMode
    {
        get => GetModelProperty("Mode")?.StringValue ?? "";
        set
        {
            SetModelProperty("Mode", value);
            OnPropertyChanged();
        }
    }

    public string Uid => GetModelProperty("UID")?.StringValue ?? "";

    public string? SelectedEnemyHealth
    {
        get => GetModelProperty("GameSetting.Vail.EnemyHealth")?.StringValue ?? "";
        set => SetModelProperty("GameSetting.Vail.EnemyHealth", value);
    }

    public string? SelectedEnemyDamage
    {
        get => GetModelProperty("GameSetting.Vail.EnemyDamage")?.StringValue ?? "";
        set => SetModelProperty("GameSetting.Vail.EnemyDamage", value);
    }

    public string? SelectedEnemyArmour
    {
        get => GetModelProperty("GameSetting.Vail.EnemyArmour")?.StringValue ?? "";
        set => SetModelProperty("GameSetting.Vail.EnemyArmour", value);
    }

    public string? SelectedEnemyAggression
    {
        get => GetModelProperty("GameSetting.Vail.EnemyAggression")?.StringValue ?? "";
        set => SetModelProperty("GameSetting.Vail.EnemyAggression", value);
    }

    public string? SelectedAnimalSpawnRate
    {
        get => GetModelProperty("GameSetting.Vail.AnimalSpawnRate")?.StringValue ?? "";
        set => SetModelProperty("GameSetting.Vail.AnimalSpawnRate", value);
    }

    public string? SelectedStartingSeason
    {
        get => GetModelProperty("GameSetting.Environment.StartingSeason")?.StringValue ?? "";
        set => SetModelProperty("GameSetting.Environment.StartingSeason", value);
    }

    public string? SelectedSeasonLength
    {
        get => GetModelProperty("GameSetting.Environment.SeasonLength")?.StringValue ?? "";
        set => SetModelProperty("GameSetting.Environment.SeasonLength", value);
    }

    public string? SelectedDayLength
    {
        get => GetModelProperty("GameSetting.Environment.DayLength")?.StringValue ?? "";
        set => SetModelProperty("GameSetting.Environment.DayLength", value);
    }

    public bool SelectedEnemySpawn
    {
        get => GetModelProperty("GameSetting.Vail.EnemySpawn")?.BoolValue ?? true;
        set => SetModelProperty("GameSetting.Vail.EnemySpawn", value);
    }

    public string? SelectedConsumableEffects
    {
        get => GetModelProperty("GameSetting.Survival.ConsumableEffects")?.StringValue ?? "Normal";
        set => SetModelProperty("GameSetting.Survival.ConsumableEffects", value);
    }

    public string? SelectedPlayerStatsDamage
    {
        get => GetModelProperty("GameSetting.Survival.PlayerStatsDamage")?.StringValue ?? "";
        set => SetModelProperty("GameSetting.Survival.PlayerStatsDamage", value);
    }

    public string? SelectedPrecipitationFrequency
    {
        get => GetModelProperty("GameSetting.Environment.PrecipitationFrequency")?.StringValue ?? "Default";
        set => SetModelProperty("GameSetting.Environment.PrecipitationFrequency", value);
    }

    private void SetModelProperty(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _gameSettings.Remove(key);
        }
        else
        {
            _gameSettings[key] = new GameSettingLightModel(key, value);
        }
    }

    private void SetModelProperty(string key, bool? value)
    {
        _gameSettings[key] = new GameSettingLightModel(key, BoolValue: value);
    }

    private GameSettingLightModel? GetModelProperty(string key)
    {
        return _gameSettings.TryGetValue(key, out var value) ? value : null;
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent m)
    {
        _gameSettings.Clear();

        foreach (var setting in m.SelectedSavegame?.SavegameStore
                                    .LoadJsonRaw(SavegameStore.FileType.GameSetupSaveData)
                                    ?.Parent.ToObject<GameSetupDataModel>()?.Data.GameSetup.Settings ??
                                Enumerable.Empty<GameSettingLightModel>())
        {
            _gameSettings.Add(setting.Name, setting);
        }

        OnPropertyChanged(nameof(SelectedMode));
        OnPropertyChanged(nameof(Uid));
        OnPropertyChanged(nameof(SelectedEnemyHealth));
        OnPropertyChanged(nameof(SelectedEnemyDamage));
        OnPropertyChanged(nameof(SelectedEnemyArmour));
        OnPropertyChanged(nameof(SelectedEnemyAggression));
        OnPropertyChanged(nameof(SelectedAnimalSpawnRate));
        OnPropertyChanged(nameof(SelectedStartingSeason));
        OnPropertyChanged(nameof(SelectedSeasonLength));
        OnPropertyChanged(nameof(SelectedDayLength));
        OnPropertyChanged(nameof(SelectedEnemySpawn));
        OnPropertyChanged(nameof(SelectedConsumableEffects));
        OnPropertyChanged(nameof(SelectedPlayerStatsDamage));
        OnPropertyChanged(nameof(SelectedPrecipitationFrequency));
    }

    public bool Update(Savegame savegame)
    {
        var saveDataWrapper = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameSetupSaveData);
        if (saveDataWrapper == null)
        {
            return false;
        }

        var hasChanges = GameSetupDataModel.Merge(saveDataWrapper, _gameSettings.Values);

        return savegame.ModifyGameState(new Dictionary<string, object>
        {
            { "GameType", SelectedMode ?? "" }
        }) || hasChanges;
    }
}