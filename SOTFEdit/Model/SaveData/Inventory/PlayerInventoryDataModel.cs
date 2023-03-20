using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverter = SOTFEdit.Infrastructure.JsonConverter;

namespace SOTFEdit.Model.SaveData.Inventory;

public record PlayerInventoryDataModel
{
    public DataModel Data { get; set; }

    public static bool Merge(JToken target, List<ItemBlockModel> selectedItems)
    {
        var playerInventoryToken = target.SelectToken("Data.PlayerInventory");
        if (playerInventoryToken?.ToObject<string>() is not { } playerInventoryJson)
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

    public class DataModel
    {
        [JsonConverter(typeof(StringTypeConverter))]
        public PlayerInventoryModel PlayerInventory { get; set; }
    }
}