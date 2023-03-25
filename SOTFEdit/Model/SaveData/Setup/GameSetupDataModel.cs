using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model.SaveData.Settings;
using JsonConverter = SOTFEdit.Infrastructure.JsonConverter;

namespace SOTFEdit.Model.SaveData.Setup;

// ReSharper disable once ClassNeverInstantiated.Global
public record GameSetupDataModel
{
    public DataModel Data { get; init; }

    public static bool Merge(JToken? targetGameSetupData, IEnumerable<GameSettingLightModel> newSettings)
    {
        if (targetGameSetupData?.SelectToken("Data.GameSetup") is not { } gameSetupToken)
        {
            return false;
        }

        if (gameSetupToken.ToString() is not { } gameSetupJson ||
            JsonConverter.DeserializeRaw(gameSetupJson) is not JObject gameSetup)
        {
            return false;
        }

        if (!GameSetupModel.Merge(gameSetup, newSettings))
        {
            return false;
        }

        gameSetupToken.Replace(JsonConverter.Serialize(gameSetup));
        return true;
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public record DataModel
    {
        [JsonConverter(typeof(StringTypeConverter))]
        public GameSetupModel GameSetup { get; init; }
    }
}