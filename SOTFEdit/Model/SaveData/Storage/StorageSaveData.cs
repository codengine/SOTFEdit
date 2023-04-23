using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SOTFEdit.Model.SaveData.Storage;

public class StorageSaveData
{
    public int Id { get; init; }
    public Position? Pos { get; init; }
    public JToken? Rot { get; init; }
    public List<StorageBlock> Storages { get; init; } = new();

    private bool Equals(StorageSaveData other)
    {
        return Id == other.Id && Storages.SequenceEqual(other.Storages);
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

        return obj.GetType() == GetType() && Equals((StorageSaveData)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Storages);
    }
}