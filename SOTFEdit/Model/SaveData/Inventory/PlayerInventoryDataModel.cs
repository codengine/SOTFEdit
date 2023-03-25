using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverter = SOTFEdit.Infrastructure.JsonConverter;

namespace SOTFEdit.Model.SaveData.Inventory;

// ReSharper disable once ClassNeverInstantiated.Global
public record PlayerInventoryDataModel
{
    public DataModel Data { get; set; }

    public static bool Merge(JToken target, IEnumerable<ItemBlockModel> selectedItems)
    {
        var playerInventoryToken = target.SelectToken("Data.PlayerInventory");
        if (playerInventoryToken?.ToString() is not { } playerInventoryJson)
        {
            return false;
        }

        var playerInventory = JsonConverter.DeserializeRaw(playerInventoryJson);
        if (!PlayerInventoryModel.Merge(playerInventory, selectedItems))
        {
            return false;
        }

        playerInventoryToken.Replace(JsonConverter.Serialize(playerInventory));
        return true;
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class DataModel
    {
        [JsonConverter(typeof(StringTypeConverter))]
        public PlayerInventoryModel PlayerInventory { get; set; }
    }
}