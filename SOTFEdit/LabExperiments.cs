using System.Linq;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit;

public static class LabExperiments
{
    public static void ResetCannibalAnger(Savegame selectedSavegame)
    {
        if (selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.VailWorldSim) is not { } vailWorldSim)
        {
            return;
        }

        foreach (var killStat in vailWorldSim["KillStatsList"] ?? Enumerable.Empty<JToken>())
        {
            killStat["PlayerKilled"]?.Replace(0);
        }

        vailWorldSim["PlayerStats"]?["CutTrees"]?.Replace(0);
        vailWorldSim["PlayerStats"]?["SeenInVillageCount"]?.Replace(0);

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.VailWorldSim);
    }
}