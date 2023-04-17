using System.Collections.Generic;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model;

public class Item
{
    private string? _normalizedLowercaseName;
    public int Id { get; init; }
    public string Name => TranslationManager.Get("items." + Id);
    public string Type { get; init; }
    public List<ItemModule>? Modules { get; init; }
    public bool IsInventoryItem { get; init; } = true;
    public bool IsEquippableArmor { get; init; } = false;
    public bool IsWearableCloth { get; init; } = false;
    public DefaultMinMax? Durability { get; init; }
    public StorageMax? StorageMax { get; init; }

    public string NormalizedLowercaseName
    {
        get { return _normalizedLowercaseName ??= TranslationHelper.Normalize(Name).ToLower(); }
    }

    private bool Equals(Item other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Item)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public record ItemModule(int ModuleId, List<int> Variants);