using Newtonsoft.Json;

namespace SOTFEdit.Model.SaveData;

public abstract record SotfBaseModel
{
    [JsonProperty(Order = -99999)] public string Version { get; init; } = "0.0.0";
}