using Newtonsoft.Json;

namespace SOTFEdit.Model.SaveData.Armour;

public record PlayerArmourDataModel : SotfBaseModel
{
    public DataModel Data { get; init; }

    public record DataModel
    {
        [JsonConverter(typeof(StringTypeConverter))]
        public PlayerArmourSystemModel PlayerArmourSystem { get; init; }
    }
}