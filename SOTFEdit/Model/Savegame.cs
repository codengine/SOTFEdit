using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.ViewModel;
using static SOTFEdit.Model.Constants.Actors;

namespace SOTFEdit.Model;

public class Savegame : ObservableObject
{
    public Savegame(string fullPath, string dirName, SavegameStore savegameStore)
    {
        SavegameStore = savegameStore;
        FullPath = fullPath;
        Title = dirName;
    }

    public string FullPath { get; }

    public SavegameStore SavegameStore { get; }

    public string Title { get; }

    public DateTime LastSaveTime => ReadLastSaveTime();
    public BitmapImage Thumbnail => BuildThumbnail();

    public string PrintableType
    {
        get
        {
            if (IsSinglePlayer())
            {
                return "SP";
            }

            if (IsMultiPlayer())
            {
                return "MP";
            }

            if (IsMultiPlayerClient())
            {
                return "MP_Client";
            }

            return "Unknown";
        }
    }

    private BitmapImage BuildThumbnail()
    {
        var thumbPath = SavegameStore.GetThumbPath() ??
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default_screenshot.png");

        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri(thumbPath);
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        image.EndInit();
        return image;
    }

    public long RegrowTrees(bool createBackup, VegetationState vegetationStateSelected)
    {
        if (SavegameStore.LoadJsonRaw(SavegameStore.FileType.WorldObjectLocatorManagerSaveData) is not JObject
            objectLocatorSaveData)
        {
            return 0;
        }

        var countRegrown = 0;

        var worldObjectLocatorManagerToken = objectLocatorSaveData.SelectToken("Data.WorldObjectLocatorManager");
        if (worldObjectLocatorManagerToken?.ToObject<string>() is not { } worldObjectLocatorManagerJson ||
            JsonConverter.DeserializeRaw(worldObjectLocatorManagerJson) is not JObject worldObjectLocatorManager)
        {
            return 0;
        }

        var serializedStates = worldObjectLocatorManager["SerializedStates"]?.ToList() ?? Enumerable.Empty<JToken>();

        foreach (var serializedState in serializedStates)
        {
            var valueToken = serializedState["Value"];
            var value = valueToken?.ToObject<short>();
            if (value == null)
            {
                continue;
            }

            var shiftedValue = (short)(1 << (value - 1));

            if (!Enum.IsDefined(typeof(VegetationState), shiftedValue) ||
                !vegetationStateSelected.HasFlag((VegetationState)shiftedValue))
            {
                continue;
            }

            serializedState.Remove();
            countRegrown++;
        }

        if (countRegrown == 0)
        {
            return 0;
        }

        worldObjectLocatorManagerToken.Replace(JsonConverter.Serialize(worldObjectLocatorManager));

        SavegameStore.StoreJson(SavegameStore.FileType.WorldObjectLocatorManagerSaveData, objectLocatorSaveData,
            createBackup);

        return countRegrown;
    }

    public bool ReviveFollower(int typeId, bool createBackup)
    {
        var reviveResult = ReviveFollowerInSaveData(typeId, createBackup);
        var gameStateKey = typeId == KelvinTypeId ? "IsRobbyDead" : "IsVirginiaDead";

        var modifyGameStateResult = ModifyGameState(new Dictionary<string, object>
        {
            { gameStateKey, false }
        }, createBackup);

        return reviveResult || modifyGameStateResult;
    }

    private bool ReviveFollowerInSaveData(int typeId, bool createBackup)
    {
        if (SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not JObject saveData)
        {
            return false;
        }

        var vailWorldSimToken = saveData.SelectToken("Data.VailWorldSim");
        if (vailWorldSimToken?.ToObject<string>() is not { } vailWorldSimJson ||
            JsonConverter.DeserializeRaw(vailWorldSimJson) is not JObject vailWorldSim)
        {
            return false;
        }

        var hasChanges = false;
        int? uniqueId = null;

        var allUniqueIds = new HashSet<int>();
        var allSpawnerIds = new HashSet<int>();

        foreach (var actor in vailWorldSim["Actors"] ?? Enumerable.Empty<JToken>())
        {
            if (actor["UniqueId"]?.ToObject<int>() is not { } actorUniqueId)
            {
                continue;
            }

            allUniqueIds.Add(actorUniqueId);

            if (actor["SpawnerId"]?.ToObject<int>() is { } spawnerId)
            {
                allSpawnerIds.Add(spawnerId);
            }

            var actorTypeId = actor["TypeId"]?.ToObject<int>();
            if (actorTypeId != typeId)
            {
                continue;
            }

            uniqueId = actorUniqueId;

            hasChanges = CompareAndModify(actor["State"], StateAlive);

            var stats = actor["Stats"];
            if (stats == null)
            {
                continue;
            }

            hasChanges = CompareAndModify(stats["Health"], f => f < FullHealth, FullHealth) || hasChanges;
            hasChanges = CompareAndModify(stats["Fear"], f => f > NoFear, NoFear) || hasChanges;
            hasChanges = CompareAndModify(stats["Anger"], f => f > NoAnger, NoAnger) || hasChanges;
            break;
        }

        foreach (var killStat in vailWorldSim["KillStatsList"] ?? Enumerable.Empty<JToken>())
        {
            if (killStat["TypeId"]?.ToObject<int>() != typeId ||
                killStat["PlayerKilled"] is not { } playerKilledToken || playerKilledToken.ToObject<int>() <= 0)
            {
                continue;
            }

            if (playerKilledToken.ToObject<int>() != 0)
            {
                playerKilledToken.Replace(0);
                hasChanges = true;
            }

            break;
        }

        if (uniqueId is { } theUniqueId)
        {
            hasChanges = ModifyFollowerData(vailWorldSim, theUniqueId) || hasChanges;
        }
        else
        {
            var npcItemInstancesToken = saveData.SelectToken("Data.NpcItemInstances");
            CreateFollowerData(vailWorldSim, npcItemInstancesToken, typeId, allUniqueIds, allSpawnerIds);
            hasChanges = true;
        }

        if (!hasChanges)
        {
            return false;
        }

        vailWorldSimToken.Replace(JsonConverter.Serialize(vailWorldSim));
        SavegameStore.StoreJson(SavegameStore.FileType.SaveData, saveData, createBackup);

        return true;
    }

    private static void CreateFollowerData(JObject vailWorldSim, JToken? npcItemInstancesToken, int typeId,
        IReadOnlySet<int> allUniqueIds,
        IReadOnlySet<int> allSpawnerIds)
    {
        var uniqueId = 1;
        while (allUniqueIds.Contains(uniqueId))
        {
            uniqueId++;
        }

        var rnd = new Random();
        var spawnerId = rnd.Next();
        while (allSpawnerIds.Contains(spawnerId))
        {
            spawnerId = rnd.Next();
        }

        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "followerTemplate.txt");
        var templateText = File.ReadAllText(templatePath, Encoding.UTF8);
        templateText = templateText.Replace(@"{uniqueId}", uniqueId.ToString());
        templateText = templateText.Replace(@"{typeId}", typeId.ToString());
        templateText = templateText.Replace(@"{spawnerId}", spawnerId.ToString());
        templateText = templateText.Replace(@"{actorSeed}", spawnerId.ToString());
        var template = JToken.Parse(templateText);

        if (template["actor"] is { } actorTemplate && vailWorldSim["Actors"] is JArray actorArray)
        {
            var playerPos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos;
            if (!playerPos.IsDefault())
            {
                actorTemplate["Position"]?.Replace(JToken.FromObject(playerPos with { Y = playerPos.Y + 2 }));
            }

            if (typeId == VirginiaTypeId)
            {
                actorTemplate["NextGiftTime"]?.Replace("1000.0");
            }

            actorArray.Add(actorTemplate);
        }

        if (template["influence"] is { } influenceTemplate && vailWorldSim["InfluenceMemory"] is JArray influenceMemory)
        {
            influenceMemory.Add(influenceTemplate);
        }

        if (template["actorItems"] is not { } actorItemTemplate || npcItemInstancesToken?.ToObject<string>() is not
                { } npcItemInstancesJson ||
            JsonConverter.DeserializeRaw(npcItemInstancesJson) is not JArray npcItemInstances)
        {
            return;
        }

        npcItemInstances.Add(actorItemTemplate);
        npcItemInstancesToken.Replace(JsonConverter.Serialize(npcItemInstances));
    }

    private static bool ModifyFollowerData(JToken vailWorldSim, int uniqueId)
    {
        var hasChanges = false;

        foreach (var influenceMemory in vailWorldSim["InfluenceMemory"] ?? Enumerable.Empty<JToken>())
        {
            if (influenceMemory["UniqueId"]?.ToObject<int>() != uniqueId)
            {
                continue;
            }

            foreach (var influenceToken in influenceMemory["Influences"] ?? Enumerable.Empty<JToken>())
            {
                if (influenceToken["TypeId"]?.ToObject<string>() != "Player")
                {
                    continue;
                }

                hasChanges = CompareAndModify(influenceToken["Sentiment"], f => f < FullSentiment, FullSentiment) ||
                             hasChanges;
                hasChanges = CompareAndModify(influenceToken["Anger"], f => f > NoAnger, NoAnger) || hasChanges;
                hasChanges = CompareAndModify(influenceToken["Fear"], f => f > NoFear, NoFear) || hasChanges;
                break;
            }

            break;
        }

        return hasChanges;
    }

    private static bool CompareAndModify(JToken? token, Func<float, bool> comparator, float newValue)
    {
        if (token == null)
        {
            return false;
        }

        var oldValue = token.ToObject<float>();
        if (!comparator.Invoke(oldValue))
        {
            return false;
        }

        token.Replace(newValue);
        return true;
    }

    private static bool CompareAndModify(JToken? token, int expectedValue)
    {
        if (token == null)
        {
            return false;
        }

        var oldValue = token.ToObject<int>();
        if (oldValue == expectedValue)
        {
            return false;
        }

        token.Replace(expectedValue);
        return true;
    }

    private DateTime ReadLastSaveTime()
    {
        if (SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData) is not JObject gameStateData)
        {
            return SavegameStore.LastWriteTime;
        }

        var gameStateToken = gameStateData.SelectToken("Data.GameState");
        if (gameStateToken?.ToObject<string>() is not { } gameStateString ||
            JsonConverter.DeserializeRaw(gameStateString) is not JObject gameState)
        {
            return SavegameStore.LastWriteTime;
        }

        return gameState["SaveTime"]?.ToObject<DateTime>() ?? SavegameStore.LastWriteTime;
    }

    public bool ModifyGameState(Dictionary<string, object> values, bool createBackup)
    {
        if (SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData) is not JObject gameStateData)
        {
            return false;
        }

        var gameStateToken = gameStateData.SelectToken("Data.GameState");
        if (gameStateToken?.ToObject<string>() is not { } gameStateString ||
            JsonConverter.DeserializeRaw(gameStateString) is not JObject gameState)
        {
            return false;
        }

        var hasChanges = false;

        foreach (var keyValuePair in values)
        {
            var token = gameState[keyValuePair.Key];
            var valueAsToken = JToken.FromObject(keyValuePair.Value);
            if (token == null || token.Equals(valueAsToken))
            {
                continue;
            }

            token.Replace(valueAsToken);
            hasChanges = true;
        }

        if (!hasChanges)
        {
            return false;
        }

        gameStateToken.Replace(JsonConverter.Serialize(gameState));
        SavegameStore.StoreJson(SavegameStore.FileType.GameStateSaveData, gameStateData, createBackup);

        return true;
    }

    public bool IsSinglePlayer()
    {
        return ParentDirIs("singleplayer");
    }

    public bool IsMultiPlayer()
    {
        return ParentDirIs("multiplayer");
    }

    public bool IsMultiPlayerClient()
    {
        return ParentDirIs("multiplayerclient");
    }

    private bool ParentDirIs(string value)
    {
        return SavegameStore.GetParentDirectory()?.Name.ToLower().Equals(value) ?? false;
    }
}