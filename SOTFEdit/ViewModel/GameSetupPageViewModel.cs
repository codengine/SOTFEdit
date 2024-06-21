using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;
using static SOTFEdit.Model.Constants;
using static SOTFEdit.Model.Constants.Settings;

namespace SOTFEdit.ViewModel;

public class GameSetupPageViewModel : ObservableObject
{
    private const string SettingsKey = "_settings";
    public const string CustomGameMode = "Custom";
    public const string CreativeGameMode = "Creative";
    public const string PeacefulGameMode = "Peaceful";
    public const string HardGameMode = "Hard";
    public const string HardSurvivalGameMode = "HardSurvival";
    public const string SettingValueHigh = "High";
    public const string SettingValueDefault = "Default";

    // ReSharper disable once InconsistentNaming
    public const string SettingValueLOW = "LOW";

    // ReSharper disable once InconsistentNaming
    public const string SettingValueNORMAL = "NORMAL";
    public const string SettingValueNormal = "Normal";
    public const string SettingValueReduced = "Reduced";
    public const string SettingValueOff = "Off";
    public const string SettingValueHard = "Hard";
    private const string NameKey = "Name";
    private const string SettingTypeKey = "SettingType";

    private readonly Dictionary<string, bool> _boolSettingDefaults = new()
    {
        { GameSetupKeys.InventoryPause, true }
    };


    private readonly Dictionary<string, bool?> _boolSettings = new();

    private readonly HashSet<string> _settingsForNonCustom = new()
    {
        GameSetupKeys.Uid, GameSetupKeys.Mode
    };

    private readonly Dictionary<string, int> _settingTypes = new()
    {
        { GameSetupKeys.Mode, SettingTypeString },
        { GameSetupKeys.Uid, SettingTypeString },
        { GameSetupKeys.EnemyHealth, SettingTypeString },
        { GameSetupKeys.EnemyDamage, SettingTypeString },
        { GameSetupKeys.EnemyArmour, SettingTypeString },
        { GameSetupKeys.EnemyAggression, SettingTypeString },
        { GameSetupKeys.AnimalSpawnRate, SettingTypeString },
        { GameSetupKeys.StartingSeason, SettingTypeString },
        { GameSetupKeys.SeasonLength, SettingTypeString },
        { GameSetupKeys.DayLength, SettingTypeString },
        { GameSetupKeys.PrecipitationFrequency, SettingTypeString },
        { GameSetupKeys.ConsumableEffects, SettingTypeString },
        { GameSetupKeys.PlayerStatsDamage, SettingTypeString },
        { GameSetupKeys.EnemySpawn, SettingTypeBool },
        { GameSetupKeys.InventoryPause, SettingTypeBool },
        { GameSetupKeys.ReducedFoodInContainers, SettingTypeBool },
        { GameSetupKeys.ReducedAmmoInContainers, SettingTypeBool },
        { GameSetupKeys.SingleUseContainers, SettingTypeBool },
        { GameSetupKeys.ColdPenalties, SettingTypeString },
        { GameSetupKeys.EnemySearchParties, SettingTypeString },
        { GameSetupKeys.PlayersTriggerTraps, SettingTypeBool },
        { GameSetupKeys.BuildingResistance, SettingTypeString },
        { GameSetupKeys.CreativeMode, SettingTypeBool },
        { GameSetupKeys.PlayersImmortalMode, SettingTypeBool },
        { GameSetupKeys.OneHitToCutTrees, SettingTypeBool },
        { GameSetupKeys.ForcePlaceFullLoad, SettingTypeBool },
        { GameSetupKeys.NoCuttingsSpawn, SettingTypeBool },
        { GameSetupKeys.PvpDamage, SettingTypeString },
        { GameSetupKeys.ColdPenaltiesStatReduction, SettingTypeString }
    };

    private readonly Dictionary<string, string> _stringSettings = new();

    public GameSetupPageViewModel()
    {
        SetupListeners();
    }

    public string? SelectedMode
    {
        get => GetStringSetting(GameSetupKeys.Mode) ?? CustomGameMode;
        set
        {
            SetStringSetting(GameSetupKeys.Mode, value);
            OnPropertyChanged();
        }
    }

    public string Uid => GetStringSetting(GameSetupKeys.Uid) ?? "";

    public string? SelectedEnemyHealth
    {
        get => GetStringSetting(GameSetupKeys.EnemyHealth) ?? SettingValueNORMAL;
        set => SetStringSetting(GameSetupKeys.EnemyHealth, value);
    }

    public string? SelectedEnemyDamage
    {
        get => GetStringSetting(GameSetupKeys.EnemyDamage) ?? SettingValueNORMAL;
        set => SetStringSetting(GameSetupKeys.EnemyDamage, value);
    }

    public string? SelectedEnemyArmour
    {
        get => GetStringSetting(GameSetupKeys.EnemyArmour) ?? SettingValueNORMAL;
        set => SetStringSetting(GameSetupKeys.EnemyArmour, value);
    }

    public string? SelectedEnemyAggression
    {
        get => GetStringSetting(GameSetupKeys.EnemyAggression) ?? SettingValueNORMAL;
        set => SetStringSetting(GameSetupKeys.EnemyAggression, value);
    }

    public string? SelectedEnemySearchParties
    {
        get => GetStringSetting(GameSetupKeys.EnemySearchParties) ?? SettingValueNORMAL;
        set => SetStringSetting(GameSetupKeys.EnemySearchParties, value);
    }

    public string? SelectedAnimalSpawnRate
    {
        get => GetStringSetting(GameSetupKeys.AnimalSpawnRate) ?? SettingValueNORMAL;
        set => SetStringSetting(GameSetupKeys.AnimalSpawnRate, value);
    }

    public string? SelectedStartingSeason
    {
        get => GetStringSetting(GameSetupKeys.StartingSeason) ?? "Summer";
        set => SetStringSetting(GameSetupKeys.StartingSeason, value);
    }

    public string? SelectedSeasonLength
    {
        get => GetStringSetting(GameSetupKeys.SeasonLength) ?? SettingValueDefault;
        set => SetStringSetting(GameSetupKeys.SeasonLength, value);
    }

    public string? SelectedDayLength
    {
        get => GetStringSetting(GameSetupKeys.DayLength) ?? SettingValueDefault;
        set => SetStringSetting(GameSetupKeys.DayLength, value);
    }

    public bool SelectedEnemySpawn
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.EnemySpawn, out var boolValue))
            {
                return boolValue ?? false;
            }

            return true;
        }
        set => SetBoolSetting(GameSetupKeys.EnemySpawn, value);
    }

    public bool SelectedInventoryPause
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.InventoryPause, out var boolValue))
            {
                return boolValue ?? true;
            }

            return true;
        }
        set => SetBoolSetting(GameSetupKeys.InventoryPause, value);
    }

    public string? SelectedConsumableEffects
    {
        get => GetStringSetting(GameSetupKeys.ConsumableEffects) ?? SettingValueNormal;
        set => SetStringSetting(GameSetupKeys.ConsumableEffects, value);
    }

    public string? SelectedPlayerStatsDamage
    {
        get => GetStringSetting(GameSetupKeys.PlayerStatsDamage) ?? SettingValueOff;
        set => SetStringSetting(GameSetupKeys.PlayerStatsDamage, value);
    }

    public string? SelectedPrecipitationFrequency
    {
        get => GetStringSetting(GameSetupKeys.PrecipitationFrequency) ?? SettingValueDefault;
        set => SetStringSetting(GameSetupKeys.PrecipitationFrequency, value);
    }

    public string? SelectedColdPenalties
    {
        get => GetStringSetting(GameSetupKeys.ColdPenalties) ?? SettingValueNormal;
        set => SetStringSetting(GameSetupKeys.ColdPenalties, value);
    }

    public string? SelectedBuildingResistance
    {
        get => GetStringSetting(GameSetupKeys.BuildingResistance) ?? SettingValueNORMAL;
        set => SetStringSetting(GameSetupKeys.BuildingResistance, value);
    }

    public string? SelectedPvpDamage
    {
        get => GetStringSetting(GameSetupKeys.PvpDamage) ?? SettingValueNormal;
        set => SetStringSetting(GameSetupKeys.PvpDamage, value);
    }
    
    public string? SelectedColdPenaltiesStatReduction
    {
        get => GetStringSetting(GameSetupKeys.ColdPenaltiesStatReduction) ?? SettingValueOff;
        set => SetStringSetting(GameSetupKeys.ColdPenaltiesStatReduction, value);
    }

    public bool SelectedReducedFoodInContainers
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.ReducedFoodInContainers, out var boolValue))
            {
                return boolValue ?? false;
            }

            return false;
        }
        set => SetBoolSetting(GameSetupKeys.ReducedFoodInContainers, value);
    }

    public bool SelectedReducedAmmoInContainers
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.ReducedAmmoInContainers, out var boolValue))
            {
                return boolValue ?? false;
            }

            return false;
        }
        set => SetBoolSetting(GameSetupKeys.ReducedAmmoInContainers, value);
    }

    public bool SelectedSingleUseContainers
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.SingleUseContainers, out var boolValue))
            {
                return boolValue ?? false;
            }

            return false;
        }
        set => SetBoolSetting(GameSetupKeys.SingleUseContainers, value);
    }

    public bool SelectedPlayersTriggerTraps
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.PlayersTriggerTraps, out var boolValue))
            {
                return boolValue ?? false;
            }

            return false;
        }
        set => SetBoolSetting(GameSetupKeys.PlayersTriggerTraps, value);
    }

    public bool SelectedCreativeMode
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.CreativeMode, out var boolValue))
            {
                return boolValue ?? false;
            }

            return false;
        }
        set => SetBoolSetting(GameSetupKeys.CreativeMode, value);
    }

    public bool SelectedPlayersImmortalMode
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.PlayersImmortalMode, out var boolValue))
            {
                return boolValue ?? false;
            }

            return false;
        }
        set => SetBoolSetting(GameSetupKeys.PlayersImmortalMode, value);
    }

    public bool SelectedOneHitToCutTrees
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.OneHitToCutTrees, out var boolValue))
            {
                return boolValue ?? false;
            }

            return false;
        }
        set => SetBoolSetting(GameSetupKeys.OneHitToCutTrees, value);
    }

    public bool SelectedForcePlaceFullLoad
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.ForcePlaceFullLoad, out var boolValue))
            {
                return boolValue ?? false;
            }

            return false;
        }
        set => SetBoolSetting(GameSetupKeys.ForcePlaceFullLoad, value);
    }

    public bool SelectedNoCuttingsSpawn
    {
        get
        {
            if (GetBoolSetting(GameSetupKeys.NoCuttingsSpawn, out var boolValue))
            {
                return boolValue ?? false;
            }

            return false;
        }
        set => SetBoolSetting(GameSetupKeys.NoCuttingsSpawn, value);
    }

    private void SetStringSetting(string key, string? value)
    {
        if (value == null)
        {
            _stringSettings.Remove(key);
        }
        else
        {
            _stringSettings[key] = value;
        }
    }

    private string? GetStringSetting(string key)
    {
        return _stringSettings.GetValueOrDefault(key);
    }

    private void SetBoolSetting(string key, bool? value)
    {
        if (value == null)
        {
            _boolSettings.Remove(key);
        }
        else
        {
            _boolSettings[key] = value;
        }
    }

    private bool GetBoolSetting(string key, out bool? value)
    {
        if (!_boolSettings.ContainsKey(key))
        {
            value = null;
            return false;
        }

        value = _boolSettings.GetValueOrDefault(key);
        return true;
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => OnSelectedSavegameChanged(m));
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent m)
    {
        _stringSettings.Clear();
        _boolSettings.Clear();

        LoadSettings(m.SelectedSavegame);

        OnPropertyChanged(nameof(SelectedMode));
        OnPropertyChanged(nameof(Uid));
        OnPropertyChanged(nameof(SelectedEnemyHealth));
        OnPropertyChanged(nameof(SelectedEnemyDamage));
        OnPropertyChanged(nameof(SelectedEnemyArmour));
        OnPropertyChanged(nameof(SelectedEnemyAggression));
        OnPropertyChanged(nameof(SelectedEnemySearchParties));
        OnPropertyChanged(nameof(SelectedAnimalSpawnRate));
        OnPropertyChanged(nameof(SelectedStartingSeason));
        OnPropertyChanged(nameof(SelectedSeasonLength));
        OnPropertyChanged(nameof(SelectedDayLength));
        OnPropertyChanged(nameof(SelectedEnemySpawn));
        OnPropertyChanged(nameof(SelectedConsumableEffects));
        OnPropertyChanged(nameof(SelectedPlayerStatsDamage));
        OnPropertyChanged(nameof(SelectedPrecipitationFrequency));
        OnPropertyChanged(nameof(SelectedInventoryPause));
        OnPropertyChanged(nameof(SelectedColdPenalties));
        OnPropertyChanged(nameof(SelectedReducedFoodInContainers));
        OnPropertyChanged(nameof(SelectedReducedAmmoInContainers));
        OnPropertyChanged(nameof(SelectedSingleUseContainers));
        OnPropertyChanged(nameof(SelectedPlayersTriggerTraps));
        OnPropertyChanged(nameof(SelectedBuildingResistance));
        OnPropertyChanged(nameof(SelectedCreativeMode));
        OnPropertyChanged(nameof(SelectedPlayersImmortalMode));
        OnPropertyChanged(nameof(SelectedOneHitToCutTrees));
        OnPropertyChanged(nameof(SelectedForcePlaceFullLoad));
        OnPropertyChanged(nameof(SelectedNoCuttingsSpawn));
        OnPropertyChanged(nameof(SelectedPvpDamage));
        OnPropertyChanged(nameof(SelectedColdPenaltiesStatReduction));
    }

    private void LoadSettings(Savegame? savegame)
    {
        if (GetGameSetupSaveData(savegame) is not { } gameSetupSaveData ||
            GetSettings(gameSetupSaveData) is not { } settings)
        {
            return;
        }

        foreach (var setting in settings)
        {
            var name = setting[NameKey]?.Value<string>();

            if (string.IsNullOrEmpty(name) || !_settingTypes.ContainsKey(name))
            {
                continue;
            }

            var settingType = (setting[SettingTypeKey] ?? setting["SettingsType"])?.Value<int>() ??
                              GuessSettingType(name);
            if (settingType == null || (settingType != SettingTypeString && settingType != SettingTypeBool))
            {
                continue;
            }

            switch (settingType)
            {
                case SettingTypeString:
                    if (SettingReader.ReadString(setting, out var stringValue))
                    {
                        if (stringValue != null)
                        {
                            _stringSettings[name] = stringValue;
                        }
                    }

                    break;
                case SettingTypeBool:
                    var boolValue = SettingReader.ReadBool(setting);
                    _boolSettings[name] = boolValue ?? _boolSettingDefaults.GetValueOrDefault(name, false);

                    break;
            }
        }
    }

    private static JToken? GetSettings(SaveDataWrapper gameSetupSaveData)
    {
        return gameSetupSaveData.GetJsonBasedToken(JsonKeys.GameSetup)?[SettingsKey];
    }

    private static SaveDataWrapper? GetGameSetupSaveData(Savegame? savegame)
    {
        return savegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameSetupSaveData);
    }

    private int? GuessSettingType(string name)
    {
        if (_settingTypes.TryGetValue(name, out var settingType))
        {
            return settingType;
        }

        return null;
    }

    public bool Update(Savegame savegame)
    {
        PrepareSettingsBeforeMerge();
        var hasChanges = Merge(savegame);

        return savegame.ModifyGameState(new Dictionary<string, object>
        {
            { "GameType", SelectedMode ?? "" }
        }) || hasChanges;
    }

    private void PrepareSettingsBeforeMerge()
    {
        if (SelectedMode == CustomGameMode)
        {
            return;
        }

        var stringKeysToRemove = _stringSettings.Keys.Where(key => !_settingsForNonCustom.Contains(key)).ToList();
        foreach (var key in stringKeysToRemove)
        {
            _stringSettings.Remove(key);
        }

        var boolKeysToRemove = _boolSettings.Keys.Where(key => !_settingsForNonCustom.Contains(key)).ToList();
        foreach (var key in boolKeysToRemove)
        {
            _boolSettings.Remove(key);
        }

        switch (SelectedMode)
        {
            case CreativeGameMode:
                _boolSettings[GameSetupKeys.EnemySpawn] = false;
                _boolSettings[GameSetupKeys.ReducedFoodInContainers] = false;
                _boolSettings[GameSetupKeys.SingleUseContainers] = false;
                _boolSettings[GameSetupKeys.InventoryPause] = true;
                _boolSettings[GameSetupKeys.PlayersTriggerTraps] = false;
                _stringSettings[GameSetupKeys.AnimalSpawnRate] = SettingValueHigh;
                _stringSettings[GameSetupKeys.ConsumableEffects] = SettingValueOff;
                _stringSettings[GameSetupKeys.PlayerStatsDamage] = SettingValueOff;
                _stringSettings[GameSetupKeys.ColdPenalties] = SettingValueOff;
                _stringSettings[GameSetupKeys.BuildingResistance] = SettingValueHigh;
                break;
            case PeacefulGameMode:
                _boolSettings[GameSetupKeys.EnemySpawn] = false;
                _stringSettings[GameSetupKeys.AnimalSpawnRate] = SettingValueHigh;
                break;
            case HardSurvivalGameMode:
                _boolSettings[GameSetupKeys.EnemySpawn] = true;
                _boolSettings[GameSetupKeys.ReducedFoodInContainers] = true;
                _boolSettings[GameSetupKeys.SingleUseContainers] = true;
                _boolSettings[GameSetupKeys.InventoryPause] = false;
                _boolSettings[GameSetupKeys.PlayersTriggerTraps] = true;
                _stringSettings[GameSetupKeys.EnemyAggression] = SettingValueHigh;
                _stringSettings[GameSetupKeys.EnemyArmour] = SettingValueHigh;
                _stringSettings[GameSetupKeys.EnemyDamage] = SettingValueHigh;
                _stringSettings[GameSetupKeys.EnemyHealth] = SettingValueNORMAL;
                _stringSettings[GameSetupKeys.AnimalSpawnRate] = SettingValueLOW;
                _stringSettings[GameSetupKeys.ConsumableEffects] = SettingValueHard;
                _stringSettings[GameSetupKeys.PlayerStatsDamage] = SettingValueHard;
                _stringSettings[GameSetupKeys.ColdPenalties] = SettingValueHard;
                break;
            case HardGameMode:
                _boolSettings[GameSetupKeys.EnemySpawn] = true;
                _stringSettings[GameSetupKeys.EnemyAggression] = SettingValueHigh;
                _stringSettings[GameSetupKeys.EnemyArmour] = SettingValueHigh;
                _stringSettings[GameSetupKeys.EnemyDamage] = SettingValueHigh;
                _stringSettings[GameSetupKeys.EnemyHealth] = SettingValueHigh;
                _stringSettings[GameSetupKeys.AnimalSpawnRate] = SettingValueLOW;
                break;
        }
    }

    private bool Merge(Savegame savegame)
    {
        if (GetGameSetupSaveData(savegame) is not { } gameSetupSaveData ||
            GetSettings(gameSetupSaveData) is not JArray settings)
        {
            return false;
        }

        List<JToken> finalSettings = new();

        var hasChanges = false;

        HashSet<string> existingSettings = new();

        foreach (var setting in settings)
        {
            var name = setting[NameKey]?.Value<string>();

            if (string.IsNullOrEmpty(name) || !_settingTypes.ContainsKey(name))
            {
                continue;
            }

            existingSettings.Add(name);

            if (!_settingTypes.ContainsKey(name))
            {
                finalSettings.Add(setting);
                hasChanges = true;
                continue;
            }

            if (_stringSettings.TryGetValue(name, out var newStringValue))
            {
                SettingReader.ReadString(setting, out var oldStringValue);
                if (oldStringValue != newStringValue)
                {
                    hasChanges = true;
                }
            }
            else if (_boolSettings.TryGetValue(name, out var newBoolSetting))
            {
                var oldBoolSetting = SettingReader.ReadBool(setting);
                if (oldBoolSetting != newBoolSetting)
                {
                    hasChanges = true;
                }
            }
        }

        foreach (var stringSetting in _stringSettings)
        {
            if (!existingSettings.Contains(stringSetting.Key))
            {
                hasChanges = true;
            }

            if (stringSetting.Value == "")
            {
                continue;
            }

            var setting = new JObject
            {
                { NameKey, stringSetting.Key },
                { SettingTypeKey, SettingTypeString },
                { SettingValueKeys.StringValue, stringSetting.Value }
            };

            finalSettings.Add(setting);
        }

        foreach (var boolSetting in _boolSettings)
        {
            if (!existingSettings.Contains(boolSetting.Key))
            {
                hasChanges = true;
            }

            var setting = new JObject
            {
                { NameKey, boolSetting.Key },
                { SettingTypeKey, SettingTypeBool },
                { SettingValueKeys.BoolValue, boolSetting.Value }
            };

            finalSettings.Add(setting);
        }

        if (hasChanges)
        {
            settings.ReplaceAll(finalSettings.ToArray());
            gameSetupSaveData.MarkAsModified(JsonKeys.GameSetup);
        }

        return hasChanges;
    }
}