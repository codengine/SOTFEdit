using System.Collections.Generic;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Storage;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class StorageDefinition : IStorageDefinition
{
    public StorageDefinition(int id, int slots, int? maxPerSlot = null, bool? preferHolder = null,
        HashSet<int>? restrictedItemIds = null,
        StorageType type = StorageType.Items)
    {
        Id = id;
        Slots = slots;
        MaxPerSlot = maxPerSlot;
        PreferHolder = preferHolder;
        RestrictedItemIds = restrictedItemIds;
        Type = type;
    }

    public bool? PreferHolder { get; }

    public int Slots { get; }
    public int? MaxPerSlot { get; }
    public HashSet<int>? RestrictedItemIds { get; }
    public string Name => TranslationManager.Get("structures.types." + Id);

    public int Id { get; }
    public StorageType Type { get; }
}

public enum StorageType
{
    Items,
    Food,
    Logs,
    Sticks,
    Stones,
    Spears,
    Bones
}