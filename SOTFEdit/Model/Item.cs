namespace SOTFEdit.Model;

public record Item
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string? NameDe { get; init; }
    public string Type { get; init; }
    public bool IsInventoryItem { get; init; } = true;
}