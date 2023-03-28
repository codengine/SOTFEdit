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
    public int Max { get; init; } = 1000;
}

// ReSharper disable once ClassNeverInstantiated.Global
public record ItemModule(int ModuleId, List<ItemVariant> Variants);

// ReSharper disable once ClassNeverInstantiated.Global
public record ItemVariant(int State, string Name);