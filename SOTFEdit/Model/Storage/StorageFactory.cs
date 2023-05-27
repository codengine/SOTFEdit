using System;
using SOTFEdit.Model.SaveData.Storage.Module;

namespace SOTFEdit.Model.Storage;

public class StorageFactory
{
    private readonly ItemList _items;

    public StorageFactory(GameData gameData)
    {
        _items = gameData.Items;
    }

    public IStorage Build(IStorageDefinition iStorageDefinition, int index)
    {
        return iStorageDefinition switch
        {
            StorageDefinition storageDefinition => Build(storageDefinition, index),
            AdvancedStorageDefinition advancedStorageDefinition => Build(advancedStorageDefinition, index),
            _ => throw new Exception("Unsupported StorageDefinition: " + iStorageDefinition.Type)
        };
    }

    private IStorage Build(StorageDefinition storageDefinition, int index)
    {
        return storageDefinition.Type switch
        {
            StorageType.Items => new ItemsStorage(storageDefinition, _items, index),
            StorageType.Food => new FoodStorage(storageDefinition, _items, index),
            StorageType.Logs => new StorageWithModulePerItem(storageDefinition, _items, index,
                () => new LogStorageModule()),
            StorageType.Sticks => new StorageWithModulePerItem(storageDefinition, _items, index),
            StorageType.Stones => new StorageWithModulePerItem(storageDefinition, _items, index),
            StorageType.Bones => new StorageWithModulePerItem(storageDefinition, _items, index),
            _ => throw new ArgumentOutOfRangeException(storageDefinition.Type.ToString())
        };
    }

    private IStorage Build(AdvancedStorageDefinition storageDefinition, int index)
    {
        return storageDefinition.Type switch
        {
            StorageType.Items => new AdvancedItemsStorage(storageDefinition, _items, index),
            _ => throw new ArgumentOutOfRangeException(storageDefinition.Type.ToString())
        };
    }
}