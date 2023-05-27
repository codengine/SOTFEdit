namespace SOTFEdit.Model.Storage;

public interface IStorageDefinition
{
    public int Id { get; }
    public StorageType Type { get; }
    public string Name { get; }
}