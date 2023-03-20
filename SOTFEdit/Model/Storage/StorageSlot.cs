using System.Collections.ObjectModel;
using System.Linq;

namespace SOTFEdit.Model.Storage;

public class StorageSlot
{
    public StorageSlot(int slot)
    {
        Slot = slot;
    }

    public int Slot { get; }
    public ObservableCollection<StoredItem> StoredItems { get; } = new();

    public int GetTotalStored()
    {
        return StoredItems.Select(storedItem => storedItem.Count)
            .Sum();
    }
}