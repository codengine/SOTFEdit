using System;
using System.Collections.Generic;
using System.Linq;

namespace SOTFEdit.Model.SaveData.Storage;

public class StorageItemBlock
{
    public int ItemId { get; init; }
    public int TotalCount { get; init; }
    public List<UniqueItem> UniqueItems { get; init; } = new();

    protected bool Equals(StorageItemBlock other)
    {
        return ItemId == other.ItemId && TotalCount == other.TotalCount && UniqueItems.SequenceEqual(other.UniqueItems);
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

        return obj.GetType() == GetType() && Equals((StorageItemBlock)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ItemId, TotalCount, UniqueItems);
    }
}