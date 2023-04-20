using SOTFEdit.Model.SaveData.Storage.Module;

namespace SOTFEdit.Model.Storage;

// ReSharper disable once NotAccessedPositionalProperty.Global
public record SourceActorStorageModule(int SourceActorType) : BaseStorageModule(1)
{
    public static int DefaultActorType => 13;
}