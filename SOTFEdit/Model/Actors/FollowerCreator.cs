using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using static SOTFEdit.Model.Constants.Actors;

namespace SOTFEdit.Model.Actors;

internal static class ActorCreator
{
    public static KeyValuePair<int, JToken>? CreateFollower(int typeId, JToken vailWorldSim, Position pos)
    {
        if (vailWorldSim["Actors"] is not JArray actorArray)
        {
            return null;
        }

        GetAllUniqueAndSpawnerIds(vailWorldSim, out var allUniqueIds, out var allSpawnerIds);
        var uniqueId = GetNextFreeUniqueId(allUniqueIds);
        var spawnerId = typeId == KelvinTypeId ? 0 : GetUnassignedSpawnerId(allSpawnerIds);

        var templateText = ReadTemplate("actorTemplate.txt")
            .Replace(@"{uniqueId}", uniqueId.ToString())
            .Replace(@"{typeId}", typeId.ToString())
            .Replace(@"{spawnerId}", spawnerId.ToString())
            .Replace(@"{actorSeed}", spawnerId.ToString());
        var actorTemplate = JToken.Parse(templateText);

        actorTemplate["Position"]?.Replace(JToken.FromObject(pos));

        if (typeId == VirginiaTypeId)
        {
            actorTemplate["NextGiftTime"]?.Replace("1000.0");
        }

        actorArray.Add(actorTemplate);

        CreateSpawner(vailWorldSim, spawnerId);

        return new KeyValuePair<int, JToken>(uniqueId, actorTemplate);
    }

    private static void CreateSpawner(JToken vailWorldSim, int spawnerId)
    {
        if (vailWorldSim["Spawners"] is not JArray spawners)
        {
            return;
        }

        var templateText = ReadTemplate("spawnerTemplate.txt")
            .Replace(@"{spawnerId}", spawnerId.ToString());

        spawners.Add(JToken.Parse(templateText));
    }

    private static int GetUnassignedSpawnerId(IReadOnlySet<int> allSpawnerIds)
    {
        if (allSpawnerIds.Count == 0)
        {
            return new Random().Next();
        }

        var spawnerId = allSpawnerIds.Max() - 1;
        while (allSpawnerIds.Contains(spawnerId) && spawnerId > int.MinValue) spawnerId--;

        return spawnerId;
    }

    private static int GetNextFreeUniqueId(IReadOnlySet<int> allUniqueIds)
    {
        var uniqueId = 1;
        while (allUniqueIds.Contains(uniqueId) && uniqueId < int.MaxValue) uniqueId++;

        return uniqueId;
    }

    private static void GetAllUniqueAndSpawnerIds(JToken vailWorldSim, out HashSet<int> allUniqueIds,
        out HashSet<int> allSpawnerIds)
    {
        allUniqueIds = new HashSet<int>();
        allSpawnerIds = new HashSet<int>();

        foreach (var actor in vailWorldSim["Actors"] ?? Enumerable.Empty<JToken>())
        {
            if (actor["UniqueId"]?.Value<int>() is { } uniqueId)
            {
                allUniqueIds.Add(uniqueId);
            }

            if (actor["SpawnerId"]?.Value<int>() is { } spawnerId)
            {
                allSpawnerIds.Add(spawnerId);
            }
        }
    }

    private static string ReadTemplate(string filename)
    {
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", filename);
        return File.ReadAllText(templatePath, Encoding.UTF8);
    }
}