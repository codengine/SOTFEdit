using SOTFEdit.Model.SaveData.Storage;

namespace SOTFEdit.Model.Storage;

public interface IStorage
{
    void SetItemsFromJson(StorageSaveData saveData);

    public StorageSaveData ToStorageSaveData();
}