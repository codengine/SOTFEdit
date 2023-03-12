using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace SOTFEdit.Infrastructure;

public static class JsonConverter
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = BuildSerializerSettings();
    private static readonly Encoding JsonEncoding = new UTF8Encoding(false);

    private static JsonSerializerSettings BuildSerializerSettings()
    {
        return new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ContractResolver = new DefaultContractResolver()
        };
    }

    public static T? DeserializeFromFile<T>(string path)
    {
        var json = File.ReadAllText(path, JsonEncoding);
        return JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings);
    }

    public static void Serialize(string targetFullPath, object model)
    {
        var json = JsonConvert.SerializeObject(model, JsonSerializerSettings);
        File.WriteAllText(targetFullPath, json, JsonEncoding);
    }

    public static JToken DeserializeRaw(string gameSetupJson)
    {
        return JToken.Parse(gameSetupJson);
    }

    public static object? DeserializeObject(string json, Type objectType)
    {
        return JsonConvert.DeserializeObject(json, objectType, JsonSerializerSettings);
    }

    public static string Serialize(object? model)
    {
        return JsonConvert.SerializeObject(model, JsonSerializerSettings);
    }
}