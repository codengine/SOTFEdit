using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.SaveData;
using SOTFEdit.Model.SaveData.Actor;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;
using static SOTFEdit.Model.Constants.Actors;

namespace SOTFEdit.Model.Actors;

public class FollowerModifier
{
    private readonly SaveDataWrapper _saveDataWrapper;

    public FollowerModifier(SaveDataWrapper saveDataWrapper)
    {
        _saveDataWrapper = saveDataWrapper;
    }

    public bool Revive(int typeId, HashSet<int> itemIds, Outfit? outfit, Position pos)
    {
        var vailWorldSim = _saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.VailWorldSim);
        if (vailWorldSim == null)
        {
            return false;
        }

        var hasChangesInVailWorldSim = false;
        var hasChangesInNpcItemInstances = false;

        Dictionary<int, JToken> uniqueIdToActorTokenForType = new();

        foreach (var actor in vailWorldSim["Actors"] ?? Enumerable.Empty<JToken>())
        {
            var actorTypeId = actor["TypeId"]?.Value<int>();
            if (actorTypeId != typeId)
            {
                continue;
            }

            if (actor["UniqueId"]?.Value<int>() is not { } actorUniqueId)
            {
                continue;
            }

            uniqueIdToActorTokenForType.Add(actorUniqueId, actor);
        }

        foreach (var (uniqueId, actor) in uniqueIdToActorTokenForType)
        {
            hasChangesInVailWorldSim =
                JsonModifier.CompareAndModify(actor["State"], StateAlive) || hasChangesInVailWorldSim;

            if (actor["Stats"] is { } stats)
            {
                hasChangesInVailWorldSim =
                    JsonModifier.CompareAndModify(stats, "Health", f => f < FullHealth, FullHealth) ||
                    hasChangesInVailWorldSim;
                hasChangesInVailWorldSim =
                    JsonModifier.CompareAndModify(stats, "Hydration", f => f < FullHydration, FullHydration) ||
                    hasChangesInVailWorldSim;
                hasChangesInVailWorldSim =
                    JsonModifier.CompareAndModify(stats, "Energy", f => f < FullEnergy, FullEnergy) ||
                    hasChangesInVailWorldSim;
                hasChangesInVailWorldSim =
                    JsonModifier.CompareAndModify(stats, "Affection", f => f > FullAffection, FullAffection) ||
                    hasChangesInVailWorldSim;
                hasChangesInVailWorldSim =
                    JsonModifier.CompareAndModify(stats, "Fear", f => f > NoFear, NoFear) ||
                    hasChangesInVailWorldSim;
            }

            hasChangesInVailWorldSim = ResetPlayerInfluence(vailWorldSim, uniqueId) || hasChangesInVailWorldSim;
        }

        hasChangesInVailWorldSim = ResetPlayerKillStatsForType(typeId, vailWorldSim) || hasChangesInVailWorldSim;

        if (uniqueIdToActorTokenForType.Count == 0)
        {
            if (ActorCreator.CreateFollower(typeId, vailWorldSim, pos) is { } kvp)
            {
                uniqueIdToActorTokenForType.Add(kvp.Key, kvp.Value);
                AddInfluencesForNewFollower(vailWorldSim, kvp.Key);
                hasChangesInVailWorldSim = true;
            }
        }

        foreach (var (uniqueId, actor) in uniqueIdToActorTokenForType)
        {
            hasChangesInVailWorldSim = EquipItemsInActor(actor, itemIds) || hasChangesInVailWorldSim;
            if (uniqueId is { })
            {
                hasChangesInNpcItemInstances =
                    EquipItemsInNpcItemInstances(uniqueId, itemIds) || hasChangesInNpcItemInstances;
            }

            hasChangesInVailWorldSim = EquipOutfit(actor, outfit) || hasChangesInVailWorldSim;
        }

        if (hasChangesInVailWorldSim)
        {
            _saveDataWrapper.MarkAsModified(Constants.JsonKeys.VailWorldSim);
        }

        if (hasChangesInNpcItemInstances)
        {
            _saveDataWrapper.MarkAsModified(Constants.JsonKeys.NpcItemInstances);
        }

        return hasChangesInVailWorldSim || hasChangesInNpcItemInstances;
    }

    private static void AddInfluencesForNewFollower(JToken vailWorldSim, int uniqueId)
    {
        AddInfluences(vailWorldSim, uniqueId, new List<Influence>
        {
            new()
            {
                TypeId = Influence.Type.Player,
                Anger = NoAnger,
                Fear = NoFear,
                Sentiment = FullSentiment
            },
            new()
            {
                TypeId = Influence.Type.Cannibal,
                Anger = FullAnger,
                Fear = FullFear,
                Sentiment = LowestSentiment
            },
            new()
            {
                TypeId = Influence.Type.Creepy,
                Anger = FullAnger,
                Fear = FullFear,
                Sentiment = LowestSentiment
            }
        });
    }

    private static void AddInfluences(JToken vailWorldSim, int uniqueId, List<Influence> influences)
    {
        if (vailWorldSim["InfluenceMemory"] is not JArray influenceMemoryToken)
        {
            influenceMemoryToken = new JArray();
            vailWorldSim["InfluenceMemory"] = influenceMemoryToken;
        }

        influenceMemoryToken.Add(JToken.FromObject(new InfluenceMemory(uniqueId, influences)));
    }

    private static bool EquipItemsInActor(JToken actor, IReadOnlySet<int> itemIds)
    {
        if (actor["EquippedItems"] is not { } oldEquippedItemsToken)
        {
            oldEquippedItemsToken = new JArray();
            actor["EquippedItems"] = oldEquippedItemsToken;
        }

        var usedOldItemIds = oldEquippedItemsToken.ToObject<HashSet<int>>() ?? new HashSet<int>();

        if (itemIds.SetEquals(usedOldItemIds))
        {
            return false;
        }

        oldEquippedItemsToken.Replace(JToken.FromObject(itemIds));
        return true;
    }

    private bool EquipItemsInNpcItemInstances(int uniqueId, IReadOnlySet<int> itemIds)
    {
        if (_saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.NpcItemInstances) is not { } npcItemInstances)
        {
            return false;
        }

        var hasChanges = false;

        if (npcItemInstances["ActorItems"] == null)
        {
            npcItemInstances["ActorItems"] = new JArray();
            hasChanges = true;
        }

        var actorItemsForActor = (npcItemInstances["ActorItems"]
            ?.Children() ?? Enumerable.Empty<JToken>()).FirstOrDefault(token =>
            token["UniqueId"]?.Value<int>() == uniqueId);

        if (actorItemsForActor is null)
        {
            actorItemsForActor = new JObject
            {
                ["UniqueId"] = uniqueId,
                ["Items"] = new JObject
                {
                    ["Version"] = "0.0.0",
                    ["ItemBlocks"] = new JArray()
                }
            };

            if (npcItemInstances["ActorItems"] is JArray actorItems)
            {
                actorItems.Add(actorItemsForActor);
                hasChanges = true;
            }
        }

        if (actorItemsForActor.SelectToken("Items.ItemBlocks") is not JArray itemBlocks)
        {
            return hasChanges;
        }

        var actorItemsToBeRemoved = itemBlocks.Where(token =>
                token["ItemId"]?.Value<int>() is { } itemId &&
                !itemIds.Contains(itemId))
            .ToList();
        actorItemsToBeRemoved.ForEach(token => token.Remove());
        hasChanges = actorItemsToBeRemoved.Count > 0 || hasChanges;

        var itemIdsExisting = new HashSet<int>();

        foreach (var itemBlock in itemBlocks)
        {
            if (itemBlock["TotalCount"] is { } totalCountToken && totalCountToken.Value<int>() < 1)
            {
                itemBlock["TotalCount"]?.Replace(1);
                hasChanges = true;
            }

            if (itemBlock["ItemId"]?.Value<int>() is { } itemId)
            {
                itemIdsExisting.Add(itemId);
            }
        }

        foreach (var itemId in itemIds.Where(itemId => !itemIdsExisting.Contains(itemId)))
        {
            itemBlocks.Add(JToken.FromObject(new ActorItemBlock(itemId, 1, new List<JToken>())));
            hasChanges = true;
        }

        return hasChanges;
    }

    private static bool EquipOutfit(JToken actor, Outfit? outfit)
    {
        var outfitIdToken = actor["OutfitId"];
        var oldOutfitId = outfitIdToken?.Value<int>() ?? 0;
        var newOutfitId = outfit?.Id ?? 0;

        if (oldOutfitId == newOutfitId)
        {
            return false;
        }

        if (newOutfitId == 0)
        {
            actor.Children<JToken>().OfType<JProperty>().FirstOrDefault(token => token.Name == "OutfitId")
                ?.Remove();
        }
        else
        {
            if (outfitIdToken == null)
            {
                actor["OutfitId"] = newOutfitId;
            }
            else
            {
                outfitIdToken.Replace(newOutfitId);
            }
        }

        return true;
    }

    private static bool ResetPlayerKillStatsForType(int typeId, JToken vailWorldSim)
    {
        var hasChanges = false;

        foreach (var killStat in vailWorldSim["KillStatsList"] ?? Enumerable.Empty<JToken>())
        {
            if (killStat["TypeId"]?.Value<int>() != typeId ||
                killStat["PlayerKilled"] is not { } playerKilledToken || playerKilledToken.Value<int>() <= 0)
            {
                continue;
            }

            if (playerKilledToken.Value<int>() != 0)
            {
                playerKilledToken.Replace(0);
                hasChanges = true;
            }

            break;
        }

        return hasChanges;
    }

    private static bool ResetPlayerInfluence(JToken vailWorldSim, int uniqueId)
    {
        var hasChanges = false;

        foreach (var influenceMemory in vailWorldSim["InfluenceMemory"] ?? Enumerable.Empty<JToken>())
        {
            if (influenceMemory["UniqueId"]?.Value<int>() != uniqueId)
            {
                continue;
            }

            foreach (var influenceToken in influenceMemory["Influences"] ?? Enumerable.Empty<JToken>())
            {
                if (influenceToken["TypeId"]?.ToString() != "Player")
                {
                    continue;
                }

                hasChanges =
                    JsonModifier.CompareAndModify(influenceToken, "Sentiment", f => f < FullSentiment, FullSentiment) ||
                    hasChanges;
                hasChanges = JsonModifier.CompareAndModify(influenceToken, "Anger", f => f > NoAnger, NoAnger) ||
                             hasChanges;
                hasChanges = JsonModifier.CompareAndModify(influenceToken, "Fear", f => f > NoFear, NoFear) ||
                             hasChanges;
                break;
            }

            break;
        }

        return hasChanges;
    }

    public bool Update(IEnumerable<FollowerState> followerStates)
    {
        var vailWorldSim = _saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.VailWorldSim);
        if (vailWorldSim == null)
        {
            return false;
        }

        var hasChangesInVailWorldSim = false;
        var hasChangesInNpcItemInstances = false;

        var statesByTypeId = followerStates.ToDictionary(state => state.TypeId);

        foreach (var actor in vailWorldSim["Actors"]?.ToList() ?? Enumerable.Empty<JToken>())
        {
            var typeId = actor["TypeId"]?.Value<int>();
            if (typeId is not { } theTypeId || !statesByTypeId.TryGetValue(theTypeId, out var followerModel))
            {
                continue;
            }

            var uniqueId = actor["UniqueId"]?.Value<int>();

            if (uniqueId == followerModel.UniqueId && actor["Position"] is { } position)
            {
                var oldPosition = position.ToObject<Position>();

                if (oldPosition != null && !oldPosition.Equals(followerModel.Pos))
                {
                    position.Replace(JToken.FromObject(followerModel.Pos));
                    hasChangesInVailWorldSim = true;
                }
            }

            var itemIds = followerModel.GetSelectedInventoryItemIds();
            hasChangesInVailWorldSim = EquipItemsInActor(actor, itemIds) || hasChangesInVailWorldSim;
            if (uniqueId is { } theUniqueId)
            {
                hasChangesInNpcItemInstances =
                    EquipItemsInNpcItemInstances(theUniqueId, itemIds) || hasChangesInNpcItemInstances;
                hasChangesInVailWorldSim = UpdateInfluenceMemory(vailWorldSim, followerModel.Influences, theUniqueId) ||
                                           hasChangesInVailWorldSim;
            }

            hasChangesInVailWorldSim = EquipOutfit(actor, followerModel.Outfit) || hasChangesInVailWorldSim;

            if (actor["Stats"] is not { } stats)
            {
                continue;
            }

            hasChangesInVailWorldSim = ModifyStat(stats, "Health", followerModel.Health) || hasChangesInVailWorldSim;
            hasChangesInVailWorldSim = ModifyStat(stats, "Anger", followerModel.Anger) || hasChangesInVailWorldSim;
            hasChangesInVailWorldSim = ModifyStat(stats, "Fear", followerModel.Fear) || hasChangesInVailWorldSim;
            hasChangesInVailWorldSim =
                ModifyStat(stats, "Fullness", followerModel.Fullness) || hasChangesInVailWorldSim;
            hasChangesInVailWorldSim =
                ModifyStat(stats, "Hydration", followerModel.Hydration) || hasChangesInVailWorldSim;
            hasChangesInVailWorldSim = ModifyStat(stats, "Energy", followerModel.Energy) || hasChangesInVailWorldSim;
            hasChangesInVailWorldSim =
                ModifyStat(stats, "Affection", followerModel.Affection) || hasChangesInVailWorldSim;
        }

        if (hasChangesInVailWorldSim)
        {
            _saveDataWrapper.MarkAsModified(Constants.JsonKeys.VailWorldSim);
        }

        if (hasChangesInNpcItemInstances)
        {
            _saveDataWrapper.MarkAsModified(Constants.JsonKeys.NpcItemInstances);
        }

        return hasChangesInVailWorldSim || hasChangesInNpcItemInstances;
    }

    private static bool UpdateInfluenceMemory(JToken vailWorldSim, IReadOnlyCollection<Influence> influences,
        int uniqueId)
    {
        if (vailWorldSim["InfluenceMemory"] is not JArray influenceMemory)
        {
            return false;
        }

        var followersInfluenceMemory =
            influenceMemory.FirstOrDefault(memory => memory["UniqueId"]?.Value<int>() == uniqueId);

        if (followersInfluenceMemory == null)
        {
            influenceMemory.Add(JToken.FromObject(new InfluenceMemory(uniqueId, new List<Influence>(influences))));
            return true;
        }

        if (followersInfluenceMemory["Influences"] is JArray followersInfluences &&
            !HasDifferencesInMemories(followersInfluences, influences))
        {
            return false;
        }

        followersInfluenceMemory["Influences"] = JToken.FromObject(influences);
        return true;
    }

    private static bool HasDifferencesInMemories(JArray existingInfluences,
        IReadOnlyCollection<Influence> newInfluences)
    {
        if (existingInfluences.Count == 0 && newInfluences.Count == 0)
        {
            return false;
        }

        if (existingInfluences.Count != newInfluences.Count)
        {
            return true;
        }

        var newInfluencesByType = newInfluences.DistinctBy(influence => influence.TypeId)
            .ToDictionary(influence => influence.TypeId);

        foreach (var existingInfluence in existingInfluences)
        {
            if (existingInfluence["TypeId"]?.ToString() is not { } typeId)
            {
                continue;
            }

            if (!newInfluencesByType.TryGetValue(typeId, out var newInfluence))
            {
                return true;
            }

            var anger = existingInfluence["Anger"]?.Value<float>() ?? 0f;
            var fear = existingInfluence["Fear"]?.Value<float>() ?? 0f;
            var sentiment = existingInfluence["Sentiment"]?.Value<float>() ?? 0f;

            if (
                Math.Abs(anger - newInfluence.Anger) > 0.001 ||
                Math.Abs(fear - newInfluence.Fear) > 0.001 ||
                Math.Abs(sentiment - newInfluence.Sentiment) > 0.001
            )
            {
                return true;
            }

            newInfluencesByType.Remove(typeId);
        }

        return newInfluencesByType.Count > 0;
    }

    private static bool ModifyStat(JToken stats, string key, float? newValue)
    {
        if (newValue == null)
        {
            return false;
        }

        if (stats[key] is { } oldValueToken && Math.Abs(oldValueToken.Value<float>() - newValue.Value) < 0.001)
        {
            return false;
        }

        stats[key] = newValue;
        return true;
    }

    public bool CreateFollowers(int typeId, int count, HashSet<int> itemIds, Outfit? outfit, Position pos)
    {
        var vailWorldSim = _saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.VailWorldSim);
        if (vailWorldSim == null || count <= 0)
        {
            return false;
        }

        var hasChangesInVailWorldSim = false;
        var hasChangesInNpcItemInstances = false;

        var basePos = new Position(pos.X + 1, pos.Y + 5, pos.Z + 1);
        var coordinates = basePos.DistributeCoordinates(count, 2, SpawnPattern.Grid);

        foreach (var (newX, newZ) in coordinates)
        {
            var usedPos = new Position(newX, basePos.Y, newZ);
            if (ActorCreator.CreateFollower(typeId, vailWorldSim, usedPos) is not { } kvp)
            {
                continue;
            }

            hasChangesInVailWorldSim = true;
            AddInfluencesForNewFollower(vailWorldSim, kvp.Key);

            hasChangesInVailWorldSim = EquipItemsInActor(kvp.Value, itemIds) || hasChangesInVailWorldSim;
            hasChangesInNpcItemInstances =
                EquipItemsInNpcItemInstances(kvp.Key, itemIds) || hasChangesInNpcItemInstances;
            hasChangesInVailWorldSim = EquipOutfit(kvp.Value, outfit) || hasChangesInVailWorldSim;

            hasChangesInVailWorldSim = EquipOutfit(kvp.Value, outfit) || hasChangesInVailWorldSim;
        }

        if (hasChangesInVailWorldSim)
        {
            _saveDataWrapper.MarkAsModified(Constants.JsonKeys.VailWorldSim);
        }

        if (hasChangesInNpcItemInstances)
        {
            _saveDataWrapper.MarkAsModified(Constants.JsonKeys.NpcItemInstances);
        }

        return hasChangesInVailWorldSim || hasChangesInNpcItemInstances;
    }
}