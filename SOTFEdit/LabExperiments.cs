using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;

namespace SOTFEdit;

public class LabExperiments
{
    public void ResetKillStatistics(Savegame selectedSavegame, bool createBackup)
    {
        LoadVeilWorldSim(selectedSavegame, out var saveDataToken, out var vailWorldSimToken, out var vailWorldSim);
        if (saveDataToken == null || vailWorldSimToken == null || vailWorldSim == null)
        {
            return;
        }

        foreach (var killStat in vailWorldSim["KillStatsList"] ?? Enumerable.Empty<JToken>())
        {
            killStat["PlayerKilled"]?.Replace(0);
        }

        vailWorldSimToken.Replace(JsonConverter.Serialize(vailWorldSim));
        selectedSavegame.SavegameStore.StoreJson(SavegameStore.FileType.SaveData, saveDataToken, createBackup);
    }

    private static void LoadVeilWorldSim(Savegame selectedSavegame, out JToken? oSaveData,
        out JToken? oVailWorldSimToken, out JToken? oVailWorldSim)
    {
        oSaveData = null;
        oVailWorldSimToken = null;
        oVailWorldSim = null;

        if (selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not JObject saveData)
        {
            return;
        }

        oSaveData = saveData;

        var vailWorldSimToken = saveData.SelectToken("Data.VailWorldSim");
        if (vailWorldSimToken?.ToObject<string>() is not { } vailWorldSimJson ||
            JsonConverter.DeserializeRaw(vailWorldSimJson) is not JObject vailWorldSim)
        {
            return;
        }

        oVailWorldSimToken = vailWorldSimToken;
        oVailWorldSim = vailWorldSim;
    }

    public void ResetNumberCutTrees(Savegame selectedSavegame, bool createBackup)
    {
        LoadVeilWorldSim(selectedSavegame, out var saveDataToken, out var vailWorldSimToken, out var vailWorldSim);
        if (saveDataToken == null || vailWorldSimToken == null || vailWorldSim == null)
        {
            return;
        }

        vailWorldSim["PlayerStats"]?["CutTrees"]?.Replace(0);

        vailWorldSimToken.Replace(JsonConverter.Serialize(vailWorldSim));
        selectedSavegame.SavegameStore.StoreJson(SavegameStore.FileType.SaveData, saveDataToken, createBackup);
    }

    public void EnemiesFearThePlayer(Savegame selectedSavegame, bool createBackup)
    {
        ModifyActors(selectedSavegame, createBackup, 0, 100);
    }

    private static void ModifyActors(Savegame selectedSavegame, bool createBackup, int anger, int fear)
    {
        LoadVeilWorldSim(selectedSavegame, out var saveDataToken, out var vailWorldSimToken, out var vailWorldSim);
        if (saveDataToken == null || vailWorldSimToken == null || vailWorldSim == null)
        {
            return;
        }

        int? kelvinUniqueId = null;
        int? virginiaUniqueId = null;

        foreach (var actor in vailWorldSim["Actors"] ?? Enumerable.Empty<JToken>())
        {
            var typeId = actor["TypeId"]?.ToObject<int>();

            switch (typeId)
            {
                case Constants.Actors.KelvinTypeId:
                    kelvinUniqueId = actor["UniqueId"]?.ToObject<int>();
                    continue;
                case Constants.Actors.VirginiaTypeId:
                    virginiaUniqueId = actor["UniqueId"]?.ToObject<int>();
                    continue;
            }

            actor.SelectToken("Stats.Fear")?.Replace(fear);
            actor.SelectToken("Stats.Anger")?.Replace(anger);
        }

        if (kelvinUniqueId != null && virginiaUniqueId != null)
        {
            foreach (var influenceMemory in vailWorldSim["InfluenceMemory"] ?? Enumerable.Empty<JToken>())
            {
                foreach (var influence in influenceMemory["Influences"] ?? Enumerable.Empty<JToken>())
                {
                    if (influence["TypeId"]?.ToObject<string>() != "Player")
                    {
                        continue;
                    }

                    influence["Anger"]?.Replace(anger);
                    influence["Fear"]?.Replace(fear);
                }
            }
        }

        vailWorldSimToken.Replace(JsonConverter.Serialize(vailWorldSim));
        selectedSavegame.SavegameStore.StoreJson(SavegameStore.FileType.SaveData, saveDataToken, createBackup);
    }

    public void EnemiesNoFearNoRemorce(Savegame selectedSavegame, bool createBackup)
    {
        ModifyActors(selectedSavegame, createBackup, 100, 0);
    }

    public void ExperimentRemoveAllActorsAndSpawns(Savegame selectedSavegame, bool createBackup)
    {
        LoadVeilWorldSim(selectedSavegame, out var saveDataToken, out var vailWorldSimToken, out var vailWorldSim);
        if (saveDataToken == null || vailWorldSimToken == null || vailWorldSim == null)
        {
            return;
        }

        int? kelvinSpawnerId = null;
        int? virginiaSpawnerId = null;

        var spawnerIds = new HashSet<long>();

        foreach (var actor in vailWorldSim["Actors"]?.ToList() ?? Enumerable.Empty<JToken>())
        {
            var typeId = actor["TypeId"]?.ToObject<int>();

            var spawnerId = actor["SpawnerId"]?.ToObject<int>();

            switch (typeId)
            {
                case Constants.Actors.KelvinTypeId:
                    kelvinSpawnerId = spawnerId;
                    continue;
                case Constants.Actors.VirginiaTypeId:
                    virginiaSpawnerId = spawnerId;
                    continue;
                default:
                    if (spawnerId is { } spawnerIdNn)
                    {
                        spawnerIds.Add(spawnerIdNn);
                    }

                    break;
            }

            actor.Remove();
        }

        if (kelvinSpawnerId != null && virginiaSpawnerId != null)
        {
            foreach (var spawner in vailWorldSim["Spawners"]?.ToList() ?? Enumerable.Empty<JToken>())
            {
                if (spawner["UniqueId"]?.ToObject<long>() is not { } spawnerUniqueId ||
                    spawnerUniqueId == kelvinSpawnerId || spawnerUniqueId == virginiaSpawnerId ||
                    !spawnerIds.Contains(spawnerUniqueId))
                {
                    continue;
                }

                spawner.Remove();
            }
        }

        vailWorldSimToken.Replace(JsonConverter.Serialize(vailWorldSim));
        selectedSavegame.SavegameStore.StoreJson(SavegameStore.FileType.SaveData, saveDataToken, createBackup);
    }
}