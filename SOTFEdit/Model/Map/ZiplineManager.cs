using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.Model.Map;

public static class ZiplineManager
{
    private const int ZiplineTypeId = 55;

    public static JObject? CreateNew(Savegame.Savegame savegame, Position anchorA, Position anchorB)
    {
        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ZipLineManagerSaveData) is
                not { } zipLinesaveDataWrapper ||
            zipLinesaveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ZipLineManager)?["Ziplines"] is not JArray
                ziplines)
        {
            return null;
        }

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureInstancesSaveData) is
                not { } screwStructureInstancesSaveDataWrapper ||
            screwStructureInstancesSaveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances)?
                ["_structures"] is not JArray screwStructureInstances)
        {
            return null;
        }

        var ziplineToken = new JObject
        {
            { "_anchorAPosition", JToken.FromObject(anchorA) },
            { "_anchorBPosition", JToken.FromObject(anchorB) },
            { "_canBeDismantled", true }
        };

        ziplines.Add(ziplineToken);

        zipLinesaveDataWrapper.MarkAsModified(Constants.JsonKeys.ZipLineManager);

        var avgX = (anchorA.X + anchorB.X) / 2;
        var avgY = (anchorA.Y + anchorB.Y) / 2;
        var avgZ = (anchorA.Z + anchorB.Z) / 2;

        var structureToken = new JObject
        {
            { "Id", ZiplineTypeId },
            { "Pos", JToken.FromObject(new Position(avgX, avgY, avgZ)) },
            { "Storages", new JArray() }
        };

        screwStructureInstances.Add(structureToken);
        screwStructureInstancesSaveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureInstances);

        return ziplineToken;
    }

    public static JToken? Move(Savegame.Savegame savegame, JToken parentToken, Position? posAOld,
        Position? posBOld,
        Position? posANew, Position? posBNew)
    {
        if (posAOld == null || posBOld == null || posANew == null || posBNew == null)
        {
            return null;
        }

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ZipLineManagerSaveData) is
                not { } zipLinesaveDataWrapper ||
            zipLinesaveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ZipLineManager)?["Ziplines"] is not JArray
                ziplines)
        {
            return null;
        }

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureInstancesSaveData) is
                not { } screwStructureInstancesSaveDataWrapper ||
            screwStructureInstancesSaveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances)?
                ["_structures"] is not JArray screwStructureInstances)
        {
            return null;
        }

        JToken? newToken = null;

        foreach (var zipline in ziplines)
        {
            if (!zipline.Equals(parentToken))
            {
                continue;
            }

            zipline["_anchorAPosition"] = JToken.FromObject(posANew);
            zipline["_anchorBPosition"] = JToken.FromObject(posBNew);
            newToken = zipline;
            break;
        }

        if (newToken == null)
        {
            return null;
        }

        zipLinesaveDataWrapper.MarkAsModified(Constants.JsonKeys.ZipLineManager);

        var oldAvgX = (posAOld.X + posBOld.X) / 2;
        var oldAvgY = (posAOld.Y + posBOld.Y) / 2;
        var oldAvgZ = (posAOld.Z + posBOld.Z) / 2;

        foreach (var screwStructureInstance in screwStructureInstances)
        {
            var pos = screwStructureInstance["Pos"];
            if (pos?["x"]?.Value<float>() is not { } avgX || pos["y"]?.Value<float>() is not { } avgY ||
                pos["z"]?.Value<float>() is not { } avgZ)
            {
                continue;
            }

            if (!(Math.Abs(oldAvgX - avgX) < 0.01) || !(Math.Abs(oldAvgY - avgY) < 0.01) ||
                !(Math.Abs(oldAvgZ - avgZ) < 0.01))
            {
                continue;
            }

            var newAvgX = (posANew.X + posBNew.X) / 2;
            var newAvgY = (posANew.Y + posBNew.Y) / 2;
            var newAvgZ = (posANew.Z + posBNew.Z) / 2;
            pos["x"] = newAvgX;
            pos["y"] = newAvgY;
            pos["z"] = newAvgZ;

            screwStructureInstancesSaveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureInstances);

            return newToken;
        }

        return null;
    }

    public static bool Remove(ZiplinePoi parent, Savegame.Savegame savegame)
    {
        if (parent.PointA.Position is not { } posA || parent.PointB.Position is not { } posB)
        {
            return false;
        }

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ZipLineManagerSaveData) is
                not { } zipLinesaveDataWrapper ||
            zipLinesaveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ZipLineManager)?["Ziplines"] is not JArray
                ziplines)
        {
            return false;
        }

        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewStructureInstancesSaveData) is
                not { } screwStructureInstancesSaveDataWrapper ||
            screwStructureInstancesSaveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewStructureInstances)?
                ["_structures"] is not JArray screwStructureInstances)
        {
            return false;
        }

        var ziplineToBeRemoved = ziplines.FirstOrDefault(zipline => zipline.Equals(parent.Token));

        if (ziplineToBeRemoved == null)
        {
            return false;
        }

        ziplines.Remove(ziplineToBeRemoved);
        zipLinesaveDataWrapper.MarkAsModified(Constants.JsonKeys.ZipLineManager);

        var oldAvgX = (posA.X + posB.X) / 2;
        var oldAvgY = (posA.Y + posB.Y) / 2;
        var oldAvgZ = (posA.Z + posB.Z) / 2;

        JToken? screwStructureToBeRemoved = null;

        foreach (var screwStructureInstance in screwStructureInstances)
        {
            if (screwStructureInstance["Id"]?.Value<int>() != ZiplineTypeId)
            {
                continue;
            }

            var pos = screwStructureInstance["Pos"];
            if (pos?["x"]?.Value<float>() is not { } avgX || pos["y"]?.Value<float>() is not { } avgY ||
                pos["z"]?.Value<float>() is not { } avgZ)
            {
                continue;
            }

            if (!(Math.Abs(oldAvgX - avgX) < 0.01) || !(Math.Abs(oldAvgY - avgY) < 0.01) ||
                !(Math.Abs(oldAvgZ - avgZ) < 0.01))
            {
                continue;
            }

            screwStructureToBeRemoved = screwStructureInstance;
            break;
        }

        if (screwStructureToBeRemoved != null)
        {
            screwStructureInstances.Remove(screwStructureToBeRemoved);
            screwStructureInstancesSaveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewStructureInstances);
        }

        return true;
    }
}