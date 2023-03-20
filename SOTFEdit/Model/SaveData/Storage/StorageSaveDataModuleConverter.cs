using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model.SaveData.Storage.Module;

namespace SOTFEdit.Model.SaveData.Storage;

public class StorageSaveDataModuleConverter : JsonConverter<IStorageModule>
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, IStorageModule? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override IStorageModule? ReadJson(JsonReader reader, Type objectType, IStorageModule? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var obj = JObject.Load(reader); // Throws an exception if the current token is not an object.
        if (obj["ModuleId"]?.ToObject<int>() is not { } moduleId)
        {
            return null;
        }

        var instance = moduleId switch
        {
            0 => BuildStickStorageModule(obj),
            3 => BuildFoodSpoilModule(moduleId, obj),
            6 => BuildLogStorageModule(moduleId, obj),
            _ => BuildGenericModule(moduleId)
        };

        using var sr = obj.CreateReader();

        serializer.Populate(sr, instance);

        return instance;
    }

    private static IStorageModule BuildGenericModule(int moduleId)
    {
        return new GenericModule(moduleId);
    }

    private static IStorageModule BuildStickStorageModule(JObject obj)
    {
        return new GenericModuleWithWeights(obj["ChannelWeights"]?.ToObject<ChannelWeightsModel>() ??
                                            new ChannelWeightsModel(0, 0, 0, 0));
    }

    private static IStorageModule BuildLogStorageModule(int moduleId, JObject obj)
    {
        return new LogStorageModule(obj["VariantNumber"]?.ToObject<int>() ??
                                    throw new Exception($"VariantNumber not defined for moduleId {moduleId}"));
    }

    private static IStorageModule BuildFoodSpoilModule(int moduleId, JObject obj)
    {
        return new FoodSpoilStorageModule(
            moduleId,
            obj["CurrentState"]?.ToObject<int>() ??
            throw new Exception($"CurrentState not defined for moduleId {moduleId}"),
            obj["TimeRemainingInState"]?.ToObject<long>() ?? default,
            obj["PauseDecay"]?.ToObject<bool>() ?? default
        );
    }
}