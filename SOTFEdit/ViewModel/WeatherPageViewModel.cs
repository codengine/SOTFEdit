using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model.Events;
using System.Threading;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;

namespace SOTFEdit.ViewModel;

public class WeatherPageViewModel
{
    private readonly ReaderWriterLockSlim _readerWriterLock = new();

    public ObservableCollection<GenericSetting> Settings { get; } = new();

    public WeatherPageViewModel()
    {
        SetupListeners();
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            Settings.Clear();
            var weatherSaveData =
                message.SelectedSavegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WeatherSystemSaveData);
            var weatherSystemToken = weatherSaveData?.SelectToken("Data.WeatherSystem");
            if (weatherSystemToken?.ToObject<string>() is not { } weatherSystemJson ||
                JsonConverter.DeserializeRaw(weatherSystemJson) is not { } weatherSystem)
            {
                return;
            }

            LoadSettings(weatherSystem);
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
    }

    private void LoadSettings(JToken weatherSystem)
    {
        var children = weatherSystem.Children();
        foreach (var child in children.OfType<JProperty>())
        {
            GenericSetting? setting = null;
            switch (child.Name)
            {
                case "_isRaining":
                case "_rainBlocked":
                    setting = new GenericSetting(child.Name, child.Path, GenericSetting.DataType.Boolean)
                    {
                        BoolValue = child.Value.ToObject<bool>()
                    };
                    break;
                case "_cloudState":
                    setting = new GenericSetting(child.Name, child.Path, GenericSetting.DataType.Enum)
                    {
                        SelectedItem = child.Value.ToObject<int>(),
                        PossibleValues =
                        {
                            { 0, "Idle" },
                            { 1, "GrowingClouds" },
                            { 2, "ReducingClouds" },
                            { 3, "Raining" }
                        }
                    };
                    break;
                case "_currentRainType":
                    setting = new GenericSetting(child.Name, child.Path, GenericSetting.DataType.Enum)
                    {
                        SelectedItem = child.Value.ToObject<int>(),
                        PossibleValues =
                        {
                            { 0, "None" },
                            { 1, "Light" },
                            { 2, "Medium" },
                            { 3, "Heavy" }
                        }
                    };
                    break;
                case "_currentSeason":
                    setting = new GenericSetting(child.Name, child.Path, GenericSetting.DataType.Enum)
                    {
                        SelectedItem = child.Value.ToObject<int>(),
                        PossibleValues =
                        {
                            { 0, "Spring" },
                            { 1, "Summer" },
                            { 2, "Autumn" },
                            { 3, "Winter" }
                        }
                    };
                    break;
            }

            if (setting != null)
            {
                Settings.Add(setting);
            }
        }
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        var weatherSaveData = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WeatherSystemSaveData);

        if (weatherSaveData?.SelectToken("Data.WeatherSystem") is not { } weatherSystemToken ||
            weatherSystemToken?.ToObject<string>() is not { } weatherSystemJson ||
            JsonConverter.DeserializeRaw(weatherSystemJson) is not { } weatherSystem)
        {
            return false;
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            if (!Merge(weatherSystem, Settings))
            {
                return false;
            }

            weatherSystemToken.Replace(JsonConverter.Serialize(weatherSystem));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        savegame.SavegameStore.StoreJson(SavegameStore.FileType.WeatherSystemSaveData, weatherSaveData, createBackup);
        return true;
    }

    private static bool Merge(JToken weatherSystem, IEnumerable<GenericSetting> settings)
    {
        return settings.Aggregate(false, (current, setting) => setting.MergeTo(weatherSystem) || current);
    }
}