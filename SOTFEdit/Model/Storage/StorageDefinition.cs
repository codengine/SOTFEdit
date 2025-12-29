using System.Collections.Generic;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Storage;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class StorageDefinition(
    int id, int slots, int? maxPerSlot = null, bool? preferHolder = null,
    HashSet<int>? restrictedItemIds = null,
    StorageType type = StorageType.Items)
    : IStorageDefinition
{
    public bool? PreferHolder { get; } = preferHolder;

    public int Slots { get; } = slots;
    public int? MaxPerSlot { get; } = maxPerSlot;
    public HashSet<int>? RestrictedItemIds { get; } = restrictedItemIds;
    public string Name => TranslationManager.Get("structures.types." + Id);

    public int Id { get; } = id;
    public StorageType Type { get; } = type;
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