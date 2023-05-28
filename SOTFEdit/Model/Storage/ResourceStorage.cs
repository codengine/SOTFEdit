using SOTFEdit.Model.SaveData.Storage;

namespace SOTFEdit.Model.Storage;

public class ResourceStorage : RestrictedStorage
{
    public ResourceStorage(StorageDefinition definition, ItemList itemList, int index) : base(definition, itemList,
        index)
    {
    }

    public override StorageSaveData ToStorageSaveData()
    {
        return ItemsStorage.GenericToStorageSaveData(Definition, Slots);
    }
}