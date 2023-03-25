using System.Collections.ObjectModel;
using System.Linq;

namespace SOTFEdit.Model.Storage;

public class StorageSlot
{
    public ObservableCollection<StoredItem> StoredItems { get; } = new();

    public int GetTotalStored()
    {
        return StoredItems.Select(storedItem => storedItem.Count)
            .Sum();
    }
}