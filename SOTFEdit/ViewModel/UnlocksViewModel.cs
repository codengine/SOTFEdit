using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using JsonConverter = SOTFEdit.Infrastructure.JsonConverter;

namespace SOTFEdit.ViewModel;

public partial class UnlocksViewModel : ObservableObject
{
    private const string SettingsKey = "_settings";
    private const string BoolValueKey = "BoolValue";
    private const string NameKey = "Name";
    private const string CreativeModeItemUnlockedKey = "Stats.CreativeModeItemUnlocked";
    private const string CoreGameCompletedKey = "Stats.CoreGameCompleted";
    private const string CreativeModeUnlockedKey = "Stats.CreativeModeUnlocked";
    private const string EscapedIslandKey = "Stats.EscapedIsland";
    private readonly ICloseable _parent;
    private readonly string _playerProfilePath;

    [ObservableProperty]
    private bool _coreGameCompleted;

    [ObservableProperty]
    private bool _creativeMode;

    [ObservableProperty]
    private bool _creativeModeItem;

    [ObservableProperty]
    private bool _escapedIsland;

    public UnlocksViewModel(string playerProfilePath, ICloseable parent)
    {
        _playerProfilePath = playerProfilePath;
        _parent = parent;
        Load(playerProfilePath);
    }

    private void Load(string playerProfilePath)
    {
        if (!File.Exists(playerProfilePath))
        {
            return;
        }

        if (JsonConverter.DeserializeJObjectFromFile(playerProfilePath)[SettingsKey] is not JArray settings)
        {
            return;
        }

        foreach (var jToken in settings)
        {
            if (jToken[NameKey]?.Value<string>() is not { } propertyName)
            {
                continue;
            }

            switch (propertyName)
            {
                case CreativeModeItemUnlockedKey:
                    CreativeModeItem = GetBoolSetting(jToken);
                    break;
                case CreativeModeUnlockedKey:
                    CreativeMode = GetBoolSetting(jToken);
                    break;
                case CoreGameCompletedKey:
                    CoreGameCompleted = GetBoolSetting(jToken);
                    break;
                case EscapedIslandKey:
                    EscapedIsland = GetBoolSetting(jToken);
                    break;
            }
        }
    }

    private static bool GetBoolSetting(JToken jToken)
    {
        return jToken[BoolValueKey]?.Value<bool>() ?? false;
    }

    [RelayCommand]
    private void Save()
    {
        var model = File.Exists(_playerProfilePath)
            ? JsonConverter.DeserializeJObjectFromFile(_playerProfilePath)
            : new JObject();

        if (model[SettingsKey] is not JArray settings)
        {
            settings = new JArray();
            model[SettingsKey] = settings;
        }

        var settingsToSave = new Dictionary<string, bool>
        {
            { CreativeModeItemUnlockedKey, CreativeModeItem },
            { CreativeModeUnlockedKey, CreativeMode },
            { CoreGameCompletedKey, CoreGameCompleted },
            { EscapedIslandKey, EscapedIsland }
        };

        foreach (var setting in settings)
        {
            if (setting[NameKey]?.Value<string>() is not { } settingName ||
                !settingsToSave.TryGetValue(settingName, out var settingValue))
            {
                continue;
            }

            setting[BoolValueKey] = settingValue;
            settingsToSave.Remove(settingName);
        }

        foreach (var (settingKey, settingValue) in settingsToSave)
        {
            var settingTpl = new JObject
            {
                [NameKey] = settingKey,
                [BoolValueKey] = settingValue
            };

            settings.Add(settingTpl);
        }

        JsonConverter.Serialize(_playerProfilePath, model, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        });

        _parent.Close();
    }
}