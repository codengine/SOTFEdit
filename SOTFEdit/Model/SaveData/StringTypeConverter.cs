using System;
using Newtonsoft.Json;

namespace SOTFEdit.Model.SaveData;

internal class StringTypeConverter : JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is string json)
        {
            return Infrastructure.JsonConverter.DeserializeObject(json, objectType);
        }

        return null;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var json = Infrastructure.JsonConverter.Serialize(value);
        serializer.Serialize(writer, json);
    }
}