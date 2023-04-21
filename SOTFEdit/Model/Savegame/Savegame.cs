using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Actors;
using static SOTFEdit.Model.Constants.Actors;

namespace SOTFEdit.Model.Savegame;

public class Savegame : ObservableObject
{
    private readonly string _dirName;

    public Savegame(string fullPath, string dirName, SavegameStore savegameStore)
    {
        SavegameStore = savegameStore;
        FullPath = fullPath;
        _dirName = dirName;
        ReadSaveData();
    }

    public string FullPath { get; }

    public SavegameStore SavegameStore { get; }

    public string Title => !string.IsNullOrWhiteSpace(GameName) ? $"{GameName} ({_dirName})" : _dirName;

    public string? GameName { get; private set; }

    public DateTime LastSaveTime { get; private set; }
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

            return TranslationManager.Get("generic.unknown");
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

    public long RegrowTrees(VegetationState vegetationStateSelected)
    {
        if (SavegameStore.LoadJsonRaw(SavegameStore.FileType.WorldObjectLocatorManagerSaveData) is not
            { } saveDataWrapper)
        {
            return 0;
        }

        var worldObjectLocatorManager = saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.WorldObjectLocatorManager);
        if (worldObjectLocatorManager is not JObject)
        {
            return 0;
        }

        var serializedStates = worldObjectLocatorManager["SerializedStates"]?.ToList() ?? Enumerable.Empty<JToken>();

        var countRegrown = 0;

        foreach (var serializedState in serializedStates)
        {
            var valueToken = serializedState["Value"];
            var value = valueToken?.Value<short>();
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

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.WorldObjectLocatorManager);

        return countRegrown;
    }

    public bool ReviveFollower(int typeId, HashSet<int> itemIds, Outfit? outfit, Position pos)
    {
        var reviveResult = ReviveFollowerInSaveData(typeId, itemIds, outfit, pos);
        var gameStateKey = typeId == KelvinTypeId ? "IsRobbyDead" : "IsVirginiaDead";

        var modifyGameStateResult = ModifyGameState(new Dictionary<string, object>
        {
            { gameStateKey, false }
        });

        return reviveResult || modifyGameStateResult;
    }

    private bool ReviveFollowerInSaveData(int typeId, HashSet<int> itemIds, Outfit? outfit, Position pos)
    {
        if (SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not { } saveDataWrapper)
        {
            return false;
        }

        var followerModifier = new FollowerModifier(saveDataWrapper);
        return followerModifier.Revive(typeId, itemIds, outfit, pos);
    }

    private void ReadSaveData()
    {
        if (SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData) is not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.GameState) is not { } gameState)
        {
            return;
        }

        LastSaveTime = gameState["SaveTime"]?.ToObject<DateTime>() ?? SavegameStore.LastWriteTime;
        if (gameState["GameName"]?.Value<string>() is { } gameName)
        {
            GameName = gameName;
        }
    }

    public bool ModifyGameState(Dictionary<string, object> values)
    {
        if (SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData) is not { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.GameState) is not JObject gameState)
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

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.GameState);

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

    public bool HasUnknownParentDir()
    {
        return !IsSinglePlayer() && !IsMultiPlayer() && !IsMultiPlayerClient();
    }

    private bool ParentDirIs(string value)
    {
        return SavegameStore.GetParentDirectory()?.Name.ToLower().Equals(value) ?? false;
    }
}