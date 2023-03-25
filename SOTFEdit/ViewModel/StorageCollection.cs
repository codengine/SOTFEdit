using System.Collections.ObjectModel;
using SOTFEdit.Model.Storage;

namespace SOTFEdit.ViewModel;

public class StorageCollection
{
    public StorageCollection(StorageDefinition storageDefinition)
    {
        StorageDefinition = storageDefinition;
    }

    public StorageDefinition StorageDefinition { get; }
    public ObservableCollection<IStorage> Storages { get; } = new();
}