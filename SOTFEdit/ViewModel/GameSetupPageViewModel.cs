using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData;
using static SOTFEdit.Model.SaveData.GameSetup;

namespace SOTFEdit.ViewModel;

public class GameSetupPageViewModel : ObservableObject
{
    private readonly Dictionary<string, GameSettingLight> _gameSettings = new();
    private readonly ReaderWriterLockSlim _readerWriterLock = new();

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

    private void SetModelProperty(string key, string? value)
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _gameSettings.Remove(key);
            }
            else
            {
                _gameSettings[key] = new GameSettingLight(key, value);
            }
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
    }

    private GameSettingLight? GetModelProperty(string key)
    {
        _readerWriterLock.EnterReadLock();
        try
        {
            return _gameSettings.TryGetValue(key, out var value) ? value : null;
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent m)
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            _gameSettings.Clear();
            foreach (var setting in m.SelectedSavegame?.SavegameStore
                                        .LoadJson<GameSetupData>(SavegameStore.FileType.GameSetupSaveData)?.Data
                                        .GameSetup.Settings ??
                                    Enumerable.Empty<GameSettingLight>())
            {
                _gameSettings.Add(setting.Name, setting);
            }
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
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
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        var saveData = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameSetupSaveData);
        if (saveData == null)
        {
            return false;
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            if (!GameSetupData.Merge(saveData, _gameSettings.Values))
            {
                return false;
            }
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        savegame.SavegameStore.StoreJson(SavegameStore.FileType.GameSetupSaveData, saveData, createBackup);

        savegame.ModifyGameState(new Dictionary<string, object>
        {
            { "GameType", SelectedMode ?? "" }
        }, createBackup);

        return true;
    }
}