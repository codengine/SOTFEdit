using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public class WeatherPageViewModel
{
    public WeatherPageViewModel()
    {
        SetupListeners();
    }

    public ObservableCollection<GenericSetting> Settings { get; } = new();

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => OnSelectedSavegameChanged(m));
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        Settings.Clear();
        var saveDataWrapper =
            message.SelectedSavegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WeatherSystemSaveData);
        if (saveDataWrapper?.GetJsonBasedToken(Constants.JsonKeys.WeatherSystem) is not { } weatherSystem)
        {
            return;
        }

        LoadSettings(weatherSystem);
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
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.Boolean, child.Path)
                    {
                        BoolValue = child.Value.Value<bool>()
                    };
                    break;
                case "_cloudState":
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.Enum, child.Path)
                    {
                        SelectedItem = child.Value.Value<int>(),
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
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.Enum, child.Path)
                    {
                        SelectedItem = child.Value.Value<int>(),
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
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.Enum, child.Path)
                    {
                        SelectedItem = child.Value.Value<int>(),
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

    public bool Update(Savegame savegame)
    {
        var saveDataWrapper = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.WeatherSystemSaveData);
        if (saveDataWrapper?.GetJsonBasedToken(Constants.JsonKeys.WeatherSystem) is not { } weatherSystem)
        {
            return false;
        }

        if (!Merge(weatherSystem, Settings))
        {
            return false;
        }

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.WeatherSystem);
        return true;
    }

    private static bool Merge(JToken weatherSystem, IEnumerable<GenericSetting> settings)
    {
        return settings.Aggregate(false, (current, setting) => setting.MergeTo(weatherSystem) || current);
    }
}