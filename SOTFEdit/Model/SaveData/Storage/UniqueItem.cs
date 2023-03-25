using System.Collections.Generic;
using System.Linq;
using SOTFEdit.Model.SaveData.Storage.Module;

namespace SOTFEdit.Model.SaveData.Storage;

public class UniqueItem
{
    public List<IStorageModule> Modules { get; init; } = new();

    private bool Equals(UniqueItem other)
    {
        return Modules.SequenceEqual(other.Modules);
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

        return obj.GetType() == GetType() && Equals((UniqueItem)obj);
    }

    public override int GetHashCode()
    {
        return Modules.GetHashCode();
    }
}