using System.Collections.Generic;
using Newtonsoft.Json;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.SaveData.Inventory;

// ReSharper disable once ClassNeverInstantiated.Global
public record PlayerInventoryDataModel
{
    public DataModel Data { get; set; }

    public static bool Merge(SaveDataWrapper saveDataWrapper, List<InventoryItem> selectedItems)
    {
        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.PlayerInventory) is not { } playerInventory)
        {
            return false;
        }

        if (!PlayerInventoryModel.Merge(playerInventory, selectedItems))
        {
            return false;
        }

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.PlayerInventory);
        return true;
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class DataModel
    {
        [JsonConverter(typeof(StringTypeConverter))]
        public PlayerInventoryModel PlayerInventory { get; set; }
    }
}