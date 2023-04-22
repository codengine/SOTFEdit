using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using SOTFEdit.ViewModel;
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

        GetAllUniqueAndSpawnerIds(vailWorldSim, out var allUniqueIds);
        var uniqueId = GetNextFreeUniqueId(allUniqueIds);

        var templateText = ReadTemplate("actorTemplate.txt")
            .Replace(@"{uniqueId}", uniqueId.ToString())
            .Replace(@"{typeId}", typeId.ToString())
            .Replace(@"{actorSeed}", new Random().Next().ToString());
        var actorTemplate = JToken.Parse(templateText);

        actorTemplate["Position"]?.Replace(JToken.FromObject(pos));

        if (typeId == VirginiaTypeId)
        {
            actorTemplate["NextGiftTime"]?.Replace("1000.0");
        }

        actorArray.Add(actorTemplate);

        return new KeyValuePair<int, JToken>(uniqueId, actorTemplate);
    }

    private static int GetNextFreeUniqueId(IReadOnlySet<int> allUniqueIds, int uniqueId = 1)
    {
        while (allUniqueIds.Contains(uniqueId) && uniqueId < int.MaxValue)
        {
            uniqueId++;
        }

        return uniqueId;
    }

    private static void GetAllUniqueAndSpawnerIds(JToken vailWorldSim, out HashSet<int> allUniqueIds)
    {
        allUniqueIds = new HashSet<int>();

        foreach (var actor in vailWorldSim["Actors"] ?? Enumerable.Empty<JToken>())
        {
            if (actor["UniqueId"]?.Value<int>() is { } uniqueId)
            {
                allUniqueIds.Add(uniqueId);
            }
        }
    }

    private static string ReadTemplate(string filename)
    {
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", filename);
        return File.ReadAllText(templatePath, Encoding.UTF8);
    }

    public static HashSet<int> CreateActors(JToken vailWorldSim, Position position, ActorType actorType, int spawnCount,
        int? familyId, int spaceBetween, SpawnPattern spawnPattern)
    {
        var newUniqueIds = new HashSet<int>();

        if (vailWorldSim["Actors"] is not JArray actorArray)
        {
            return newUniqueIds;
        }

        GetAllUniqueAndSpawnerIds(vailWorldSim, out var allUniqueIds);

        var templateText = ReadTemplate("actorTemplate.txt")
            .Replace(@"{uniqueId}", "0")
            .Replace(@"{typeId}", actorType.Id.ToString())
            .Replace(@"{actorSeed}", "0");
        var actorTemplate = JToken.Parse(templateText);

        actorTemplate["Position"] = JToken.FromObject(position);
        if (familyId != null)
        {
            actorTemplate["FamilyId"] = familyId;
        }

        actorTemplate["GraphMask"] = position.Area.GraphMask;

        var nextUniqueId = 1;

        List<Tuple<float, float>>? distributedCoordinates = null;

        if (spaceBetween > 0 && spawnCount > 0)
        {
            distributedCoordinates = position.DistributeCoordinates(spawnCount, spaceBetween, spawnPattern);
        }

        var random = new Random();

        for (var i = 0; i < spawnCount; i++)
        {
            var cloned = actorTemplate.DeepClone();

            nextUniqueId = GetNextFreeUniqueId(allUniqueIds, nextUniqueId);
            allUniqueIds.Add(nextUniqueId);
            newUniqueIds.Add(nextUniqueId);
            cloned["UniqueId"] = nextUniqueId;
            cloned["ActorSeed"] = random.Next();

            if (distributedCoordinates != null)
            {
                if (i >= distributedCoordinates.Count)
                {
                    continue;
                }

                var (newX, newZ) = distributedCoordinates[i];
                cloned["Position"] = JToken.FromObject(new Position(newX, position.Y, newZ));
            }

            actorArray.Add(cloned);
        }

        return newUniqueIds;
    }
}