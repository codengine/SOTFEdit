using SOTFEdit.Model.SaveData.Storage;

namespace SOTFEdit.Model.Storage;

public interface IStorage
{
    public string Description { get; }
    void SetItemsFromJson(StorageSaveData saveData);

    public StorageSaveData ToStorageSaveData();
    void SetAllToMax();
}