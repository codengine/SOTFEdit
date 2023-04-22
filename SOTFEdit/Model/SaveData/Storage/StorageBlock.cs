using System.Collections.Generic;
using System.Linq;

namespace SOTFEdit.Model.SaveData.Storage;

public class StorageBlock
{
    public List<StorageItemBlock> ItemBlocks { get; init; } = new();

    // ReSharper disable once UnusedMember.Global
    public string Version => "0.0.0";

    private bool Equals(StorageBlock other)
    {
        return ItemBlocks.SequenceEqual(other.ItemBlocks);
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

        return obj.GetType() == GetType() && Equals((StorageBlock)obj);
    }

    public override int GetHashCode()
    {
        return ItemBlocks.GetHashCode();
    }
}