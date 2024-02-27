using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace SOTFEdit.Infrastructure;

public static class JsonConverter
{
    private static readonly JsonSerializerSettings DefaultJsonSerializerSettings = BuildDefaultSerializerSettings();
    private static readonly Encoding JsonEncoding = new UTF8Encoding(false);

    private static JsonSerializerSettings BuildDefaultSerializerSettings()
    {
        return new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ContractResolver = new DefaultContractResolver()
        };
    }

    public static T? DeserializeFromFile<T>(string path)
    {
        return DeserializeFromFile<T>(path, DefaultJsonSerializerSettings);
    }

    public static T? DeserializeFromFile<T>(string path, JsonSerializerSettings serializerSettings)
    {
        var json = File.ReadAllText(path, JsonEncoding);
        return JsonConvert.DeserializeObject<T>(json, serializerSettings);
    }

    public static void Serialize(string targetFullPath, object model)
    {
        Serialize(targetFullPath, model, DefaultJsonSerializerSettings);
    }

    public static void Serialize(string targetFullPath, object model, JsonSerializerSettings serializerSettings)
    {
        var json = JsonConvert.SerializeObject(model, serializerSettings);
        File.WriteAllText(targetFullPath, json, JsonEncoding);
    }

    public static JToken DeserializeRaw(string json)
    {
        return JToken.Parse(json);
    }

    public static object? DeserializeObject(string json, Type objectType)
    {
        return JsonConvert.DeserializeObject(json, objectType, DefaultJsonSerializerSettings);
    }

    public static string Serialize(object? model)
    {
        return JsonConvert.SerializeObject(model, DefaultJsonSerializerSettings);
    }

    public static T? DeserializeFromString<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, DefaultJsonSerializerSettings);
    }

    public static JToken DeserializeJObjectFromFile(string path)
    {
        var json = File.ReadAllText(path, JsonEncoding);
        return JObject.Parse(json);
    }
}