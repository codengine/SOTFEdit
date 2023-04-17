using System.Collections.Generic;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Storage;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class StorageDefinition
{
    public StorageDefinition(int id, int slots, int? maxPerSlot = null,
        HashSet<int>? restrictedItemIds = null,
        StorageType type = StorageType.Items)
    {
        Id = id;
        Slots = slots;
        MaxPerSlot = maxPerSlot;
        RestrictedItemIds = restrictedItemIds;
        Type = type;
    }

    public int Id { get; }
    public string Name => TranslationManager.Get("structures.types." + Id);
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