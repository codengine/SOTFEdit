using System.Collections.Generic;

namespace SOTFEdit.Model.Storage;

public sealed class StorageDefinition
{
    public StorageDefinition(int id, string name, int slots, int? maxPerSlot = null,
        HashSet<int>? restrictedItemIds = null,
        StorageType type = StorageType.Items)
    {
        Id = id;
        Name = name;
        Slots = slots;
        MaxPerSlot = maxPerSlot;
        RestrictedItemIds = restrictedItemIds;
        Type = type;
    }

    public int Id { get; }
    public string Name { get; }
    public int Slots { get; }
    public int? MaxPerSlot { get; }
    public HashSet<int>? RestrictedItemIds { get; }
    public StorageType Type { get; }
}

public enum StorageType
{
    Items,
    Food,
    Logs,
    Sticks,
    Stones,
    Bones
}