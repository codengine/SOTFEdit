using System;
using Newtonsoft.Json;
using SOTFEdit.Model.SaveData.Storage.Module;

namespace SOTFEdit.Model.SaveData.Storage;

public class GenericStorageModuleConverter : JsonConverter<GenericModule>
{
    public override bool CanWrite => true;
    public override bool CanRead => false;

    public override void WriteJson(JsonWriter writer, GenericModule? value, JsonSerializer serializer)
    {
        value?.ModuleToken.WriteTo(writer);
    }

    public override GenericModule ReadJson(JsonReader reader, Type objectType, GenericModule? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}