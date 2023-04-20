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

    public IStorage Build(StorageDefinition storageDefinition, int index)
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
}