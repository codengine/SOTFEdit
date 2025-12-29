using System.Collections.ObjectModel;
using SOTFEdit.Model.Storage;

namespace SOTFEdit.ViewModel;

public class StorageCollection(int storageTypeId, string name)
{
    public int StorageTypeId { get; } = storageTypeId;
    public string Name { get; } = name;

    public ObservableCollection<IStorage> Storages { get; } = [];
}