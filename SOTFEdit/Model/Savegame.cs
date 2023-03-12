using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using static SOTFEdit.Model.Constants.Actors;

namespace SOTFEdit.Model;

public class Savegame
{
    private readonly string _id;

    public Savegame(string id, SavegameStore savegameStore)
    {
        SavegameStore = savegameStore;
        _id = id;
    }

    public SavegameStore SavegameStore { get; }

    public string Title => _id + (IsMultiplayer ? " (MP)" : " (SP)");

    public string ThumbPath => SavegameStore.GetThumbPath();

    public bool IsMultiplayer => SavegameStore.IsMultiplayer();

    public void RegrowTrees(bool createBackup)
    {
        if (createBackup)
        {
            SavegameStore.MoveToBackup(SavegameStore.FileType.WorldObjectLocatorManagerSaveData);
        }
        else
        {
            SavegameStore.Delete(SavegameStore.FileType.WorldObjectLocatorManagerSaveData);
        }
    }

    public bool ReviveFollowers(bool createBackup)
    {
        var reviveResult = ReviveFollowersInSaveData(createBackup);
        var modifyGameStateResult = ModifyGameState(new Dictionary<string, object>
        {
            { "IsRobbyDead", false },
            { "IsVirginiaDead", false }
        }, createBackup);

        return reviveResult || modifyGameStateResult;
    }

    private bool ReviveFollowersInSaveData(bool createBackup)
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

        foreach (var actor in vailWorldSim["Actors"] ?? Enumerable.Empty<JToken>())
        {
            var typeId = actor["TypeId"]?.ToObject<int>();
            if (typeId is not (KelvinTypeId or VirginiaTypeId))
            {
                continue;
            }

            var stateToken = actor["State"];
            if (stateToken != null && stateToken.ToObject<int>() != StateAlive)
            {
                stateToken.Replace(StateAlive);
                hasChanges = true;
            }

            var healthToken = actor.SelectToken("Stats.Health");
            if (healthToken == null || !(healthToken.ToObject<float>() < FullHealth))
            {
                continue;
            }

            healthToken.Replace(FullHealth);
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
}