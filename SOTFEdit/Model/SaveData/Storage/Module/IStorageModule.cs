using Newtonsoft.Json;

namespace SOTFEdit.Model.SaveData.Storage.Module;

[JsonConverter(typeof(StorageSaveDataModuleConverter))]
public interface IStorageModule
{
    public int GetModuleId();
    bool IsEqualTo(IStorageModule? other);
}