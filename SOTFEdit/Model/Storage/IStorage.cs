using SOTFEdit.Model.SaveData.Storage;

namespace SOTFEdit.Model.Storage;

public interface IStorage
{
    public string Description { get; }
    void SetSaveData(StorageSaveData saveData);

    public StorageSaveData ToStorageSaveData();
    void SetAllToMax();
    int GetStorageTypeId();
    void ApplyFrom(IStorage messageStorage);
}