using System;

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
        switch (storageDefinition.Type)
        {
            case StorageType.Items:
                return new ItemsStorage(storageDefinition, _items, index);
            case StorageType.Food:
                return new FoodStorage(storageDefinition, _items, index);
            case StorageType.Logs:
            case StorageType.Sticks:
            case StorageType.Stones:
            case StorageType.Spears:
            case StorageType.Bones:
                return new ResourceStorage(storageDefinition, _items, index);
            default:
                throw new ArgumentOutOfRangeException(storageDefinition.Type.ToString());
        }
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