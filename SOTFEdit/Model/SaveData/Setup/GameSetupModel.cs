using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model.SaveData.Settings;

namespace SOTFEdit.Model.SaveData.Setup;

public record GameSetupModel
{
    private const string SettingsKey = "_settings";
    [JsonProperty(SettingsKey)] public List<GameSettingLightModel> Settings;

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
            if (name == null)
            {
                finalSettings.Add(setting); //we don't know what it is, so we just keep it
                hasChanges = true;
                continue;
            }

            if (newSettingsDict.TryGetValue(name, out var value))
            {
                var stringValueToken = setting["StringValue"];
                var newStringValueToken = JToken.FromObject(value.StringValue);
                if (stringValueToken != null && !stringValueToken.Equals(newStringValueToken))
                {
                    stringValueToken.Replace(newStringValueToken);
                    hasChanges = true;
                }

                finalSettings.Add(setting);
            }
            else
            {
                hasChanges = true;
            }

            processedKeys.Add(name);
        }

        var newSettingsFromDict = newSettingsDict.Where(kvp => !processedKeys.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .Select(setting => JToken.FromObject(new GameSettingFullModel
            {
                Name = setting.Name,
                StringValue = setting.StringValue
            })).ToList();

        hasChanges = hasChanges || newSettingsFromDict.Any();

        finalSettings.AddRange(newSettingsFromDict);
        if (hasChanges)
        {
            settings.ReplaceAll(finalSettings.ToArray());
        }

        return hasChanges;
    }
}