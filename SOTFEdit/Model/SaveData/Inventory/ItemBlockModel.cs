using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SOTFEdit.Model.SaveData.Inventory;

public class ItemBlockModel
{
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
}