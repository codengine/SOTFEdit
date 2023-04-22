using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model.SaveData.Settings;
using static SOTFEdit.Model.Constants.Settings;

namespace SOTFEdit.Model.SaveData.Setup;

public record GameSetupModel
{
    private const string SettingsKey = "_settings";

    [JsonProperty(SettingsKey)]
    public List<GameSettingLightModel> Settings;

    internal static bool Merge(JObject gameSetup, IEnumerable<GameSettingLightModel> newSettings)
    {
        if (gameSetup[SettingsKey] is not JArray settings)
        {
            return false;
        }

        var newSettingsDict = newSettings.ToDictionary(setting => setting.Name);
        List<JToken> finalSettings = new();
        HashSet<string> processedKeys = new();

        var hasChanges = false;

        foreach (var setting in settings)
        {
            var name = setting["Name"]?.ToString();
            if (name != null)
            {
                processedKeys.Add(name);
            }

            var settingType = setting["SettingType"]?.Value<int>();
            if (name == null || settingType is not (SettingTypeString or SettingTypeBool))
            {
                finalSettings.Add(setting); //we don't know what it is, so we just keep it
                hasChanges = true;
                continue;
            }

            if (newSettingsDict.TryGetValue(name, out var value))
            {
                JToken? valueToken;
                JToken newTokenValue;

                switch (settingType)
                {
                    case SettingTypeBool:
                        valueToken = setting["BoolValue"];
                        newTokenValue = JToken.FromObject(value.BoolValue ?? false);
                        break;
                    case SettingTypeString:
                        valueToken = setting["StringValue"];
                        newTokenValue = JToken.FromObject(value.StringValue ?? "");
                        break;
                    default:
                        finalSettings.Add(setting);
                        continue;
                }


                if (valueToken != null && !valueToken.Equals(newTokenValue))
                {
                    valueToken.Replace(newTokenValue);
                    hasChanges = true;
                }

                finalSettings.Add(setting);
            }
            else
            {
                hasChanges = true;
            }
        }

        var newSettingsFromDict = newSettingsDict.Where(kvp => !processedKeys.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .Select(setting =>
            {
                if (setting.StringValue is { } stringValue)
                {
                    return JToken.FromObject(new GameSettingFullModel
                    {
                        Name = setting.Name,
                        StringValue = stringValue
                    });
                }

                if (setting.BoolValue is { } boolValue)
                {
                    return JToken.FromObject(new GameSettingFullModel
                    {
                        Name = setting.Name,
                        SettingsType = SettingTypeBool,
                        BoolValue = boolValue
                    });
                }

                return null;
            }).Where(setting => setting != null)
            .Select(setting => setting!)
            .ToList();

        hasChanges = hasChanges || newSettingsFromDict.Any();

        finalSettings.AddRange(newSettingsFromDict);
        if (hasChanges)
        {
            settings.ReplaceAll(finalSettings.ToArray());
        }

        return hasChanges;
    }
}