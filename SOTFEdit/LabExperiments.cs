using System.Linq;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit;

public static class LabExperiments
{
    public static void ResetKillStatistics(Savegame selectedSavegame)
    {
        if (selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.VailWorldSim) is not { } vailWorldSim)
        {
            return;
        }

        foreach (var killStat in vailWorldSim["KillStatsList"] ?? Enumerable.Empty<JToken>())
            killStat["PlayerKilled"]?.Replace(0);

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.VailWorldSim);
    }

    public static void ResetNumberCutTrees(Savegame selectedSavegame)
    {
        if (selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.VailWorldSim) is not { } vailWorldSim)
        {
            return;
        }

        vailWorldSim["PlayerStats"]?["CutTrees"]?.Replace(0);

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.VailWorldSim);
    }
}