using System.Collections.Generic;

namespace SOTFEdit.Model.SaveData.Inventory;

// ReSharper disable once ClassNeverInstantiated.Global
public record ItemInstanceManagerDataModel : SotfBaseModel
{
    public List<ItemBlockModel> ItemBlocks { get; set; }
}