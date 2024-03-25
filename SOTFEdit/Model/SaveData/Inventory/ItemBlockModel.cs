using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SOTFEdit.Model.SaveData.Inventory;

public class ItemBlockModel
{
    public List<JToken> Modules = new(); //Only used by equipped items...hopefully

    // ReSharper disable once UnusedMember.Global
    public List<JToken> UniqueItems = new();
    public int ItemId { get; set; }
    public int TotalCount { get; set; }

    public static ItemBlockModel Unassigned(Item item)
    {
        return new ItemBlockModel
        {
            ItemId = item.Id,
            TotalCount = item.StorageMax?.Inventory ?? 1
        };
    }

    // ReSharper disable once UnusedMember.Global
#pragma warning disable CA1822
    public bool ShouldSerializeModules()
#pragma warning restore CA1822
    {
        return false;
    }
}