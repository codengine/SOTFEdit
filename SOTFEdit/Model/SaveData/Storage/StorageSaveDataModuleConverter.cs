using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.SaveData.Storage.Module;
using SOTFEdit.Model.Storage;

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
        if (obj["ModuleId"]?.Value<int>() is not { } moduleId)
        {
            return new GenericModule(-1, obj);
        }

        var instance = moduleId switch
        {
            1 => BuildSourceActorStorageModule(obj),
            3 => BuildFoodSpoilModule(moduleId, obj),
            _ => BuildGenericModule(moduleId, obj)
        };

        using var sr = obj.CreateReader();

        serializer.Populate(sr, instance);

        return instance;
    }

    private static IStorageModule BuildSourceActorStorageModule(JObject obj)
    {
        return new SourceActorStorageModule(obj["SourceActorType"]?.Value<int>() ??
                                            SourceActorStorageModule.DefaultActorType);
    }

    private static IStorageModule BuildGenericModule(int moduleId, JObject moduleToken)
    {
        return new GenericModule(moduleId, moduleToken);
    }

    private static IStorageModule BuildFoodSpoilModule(int moduleId, JObject obj)
    {
        return new FoodSpoilStorageModule(
            moduleId,
            obj["CurrentState"]?.Value<int>() ??
            throw new Exception(
                TranslationManager.GetFormatted("storage.errors.currentStateNotDefined", moduleId)),
            obj["TimeRemainingInState"]?.Value<long>() ?? default,
            obj["PauseDecay"]?.Value<bool>() ?? default
        );
    }
}