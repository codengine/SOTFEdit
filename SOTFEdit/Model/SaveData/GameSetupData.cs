using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static SOTFEdit.Model.SaveData.GameSetup;
using JsonConverter = SOTFEdit.Infrastructure.JsonConverter;

namespace SOTFEdit.Model.SaveData;

public record GameSetupData
{
    public DataModel Data { get; init; }

    public static bool Merge(JToken? targetGameSetupData, IEnumerable<GameSettingLight> newSettings)
    {
        if (targetGameSetupData?.SelectToken("Data.GameSetup") is not { } gameSetupToken)
        {
            return false;
        }

        if (gameSetupToken.ToObject<string>() is not { } gameSetupJson ||
            JsonConverter.DeserializeRaw(gameSetupJson) is not JObject gameSetup)
        {
            return false;
        }

        if (!GameSetup.Merge(gameSetup, newSettings))
        {
            return false;
        }

        gameSetupToken.Replace(JsonConverter.Serialize(gameSetup));
        return true;
    }

    public record DataModel
    {
        [JsonConverter(typeof(StringTypeConverter))]
        public GameSetup GameSetup { get; init; }
    }
}

public record GameSetup
{
    private const string SettingsKey = "_settings";
    [JsonProperty(SettingsKey)] public List<GameSettingLight> Settings;


    internal static bool Merge(JObject gameSetup, IEnumerable<GameSettingLight> newSettings)
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
            var name = setting["Name"]?.ToObject<string>();
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
            .Select(setting => JToken.FromObject(new GameSettingFull
            {
                Name = setting.Name, StringValue = setting.StringValue
            })).ToList();

        hasChanges = hasChanges || newSettingsFromDict.Any();

        finalSettings.AddRange(newSettingsFromDict);
        if (hasChanges)
        {
            settings.ReplaceAll(finalSettings.ToArray());
        }

        return hasChanges;
    }

    public record GameSettingLight(string Name, string StringValue);
}

public record GameSettingFull
{
    public string Name { get; set; }
    public int SettingsType { get; set; } = 3;
    public int Version { get; set; } = 0;
    public bool BoolValue { get; set; } = false;
    public int IntValue { get; set; } = 0;
    public decimal FloatValue { get; set; } = new(0.0);
    public string StringValue { get; set; }
    public bool Protected { get; set; } = false;
    public decimal[] FloatArrayValue { get; set; } = Array.Empty<decimal>();
    public bool IsSet { get; set; } = false;
}