using System.Collections.Generic;

namespace SOTFEdit.Model;

public class Item
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string? NameDe { get; init; }
    public string Type { get; init; }
    public List<ItemModule>? Modules { get; init; }
    public bool IsInventoryItem { get; init; } = true;
    public bool IsEquippableArmor { get; init; } = false;
    public bool IsWearableCloth { get; init; } = false;
    public DefaultMinMax? Durability { get; init; }
    public StorageMax? StorageMax { get; init; }

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
public record ItemModule(int ModuleId, List<ItemVariant> Variants);

// ReSharper disable once ClassNeverInstantiated.Global
public record ItemVariant(int State, string Name);