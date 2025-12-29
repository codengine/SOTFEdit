using SOTFEdit.Model.SaveData.Storage;

namespace SOTFEdit.Model.Storage;

public class ResourceStorage(StorageDefinition definition, ItemList itemList, int index) : RestrictedStorage(definition,
    itemList,
    index)
{
    public override StorageSaveData ToStorageSaveData()
    {
        return ItemsStorage.GenericToStorageSaveData(Definition, Slots);
    }
}