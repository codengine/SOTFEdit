using System.Collections.ObjectModel;
using SOTFEdit.Model.Storage;

namespace SOTFEdit.ViewModel;

public class StorageCollection
{
    public StorageCollection(int storageTypeId, string name)
    {
        StorageTypeId = storageTypeId;
        Name = name;
    }

    public int StorageTypeId { get; }
    public string Name { get; }

    public ObservableCollection<IStorage> Storages { get; } = new();
}