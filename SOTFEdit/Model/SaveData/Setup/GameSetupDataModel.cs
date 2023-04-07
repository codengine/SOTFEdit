using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model.SaveData.Settings;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.Model.SaveData.Setup;

// ReSharper disable once ClassNeverInstantiated.Global
public record GameSetupDataModel
{
    public DataModel Data { get; init; }

    public static bool Merge(SaveDataWrapper saveDataWrapper, IEnumerable<GameSettingLightModel> newSettings)
    {
        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.GameSetup) is not JObject gameSetup ||
            !GameSetupModel.Merge(gameSetup, newSettings))
        {
            return false;
        }

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.GameSetup);
        return true;
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public record DataModel
    {
        [JsonConverter(typeof(StringTypeConverter))]
        public GameSetupModel GameSetup { get; init; }
    }
}