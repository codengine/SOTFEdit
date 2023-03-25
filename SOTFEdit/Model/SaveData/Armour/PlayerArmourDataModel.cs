using Newtonsoft.Json;

namespace SOTFEdit.Model.SaveData.Armour;

// ReSharper disable once ClassNeverInstantiated.Global
public record PlayerArmourDataModel : SotfBaseModel
{
    public DataModel Data { get; init; }

    // ReSharper disable once ClassNeverInstantiated.Global
    public record DataModel
    {
        [JsonConverter(typeof(StringTypeConverter))]
        public PlayerArmourSystemModel PlayerArmourSystem { get; init; }
    }
}