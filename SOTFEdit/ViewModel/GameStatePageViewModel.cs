using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public class GameStatePageViewModel
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly GameData _gameData;

    private readonly Regex _resetCrateNameIdPattern =
        new(@"(\..*Crate.*\.)|(\..*Storage.*\.)|(\..*Case.*\.)|(.*\.Meds\..*)");

    public GameStatePageViewModel(GameData gameData)
    {
        _gameData = gameData;
        PrefillNamedIntDatas(gameData.NamedIntKeys);
        SetupListeners();
    }

    public ObservableCollection<GenericSetting> Settings { get; } = new();
    public ObservableCollection<GenericSetting> NamedIntDatas { get; } = new();

    private void PrefillNamedIntDatas(List<string> namedIntKeys)
    {
        foreach (var namedIntKey in namedIntKeys)
        {
            NamedIntDatas.Add(new GenericSetting(namedIntKey, GenericSetting.DataType.Integer)
            {
                IntValue = null,
                MaxInt = 3
            });
        }
    }

    private void OnRequestResetContainers()
    {
        var filtered = NamedIntDatas.Where(setting => !_resetCrateNameIdPattern.Match(setting.Name).Success)
            .ToList();

        var numFixed = NamedIntDatas.Count - filtered.Count;

        if (numFixed < 1)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.Get("windows.resetContainers.messages.nothingToReset.text"),
                TranslationManager.Get("windows.resetContainers.messages.nothingToReset.title")
            ));
            return;
        }

        NamedIntDatas.Clear();

        filtered.ForEach(setting => NamedIntDatas.Add(setting));

        WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
            TranslationManager.GetFormatted("windows.resetContainers.messages.containersReset.text", numFixed),
            TranslationManager.Get("windows.resetContainers.messages.containersReset.title")
        ));
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => OnSelectedSavegameChanged(m));
        WeakReferenceMessenger.Default.Register<RequestResetContainersEvent>(this,
            (_, _) => OnRequestResetContainers());
        WeakReferenceMessenger.Default.Register<RequestSetToEndgameEvent>(this,
            (_, _) => OnRequestSetToEndgameEvent());
    }

    private void OnRequestSetToEndgameEvent()
    {
        GenericSetting? stayedOnIslandSetting = null;
        GenericSetting? coreGameCompletedSetting = null;
        
        foreach (var namedIntData in NamedIntDatas)
        {
            switch (namedIntData.Name)
            {
                case "Endgame.StayedOnIsland":
                    stayedOnIslandSetting = namedIntData;
                    stayedOnIslandSetting.IntValue = 1;
                    break;
                case "Endgame.CoreGameCompleted":
                    coreGameCompletedSetting = namedIntData;
                    coreGameCompletedSetting.IntValue = 1;
                    break;
            }
        }

        if (stayedOnIslandSetting == null)
        {
            var namedIntData = CreateNamedIntData("Endgame.StayedOnIsland");
            namedIntData.IntValue = 1;
        }
        
        if (coreGameCompletedSetting == null)
        {
            var namedIntData = CreateNamedIntData("Endgame.CoreGameCompleted");
            namedIntData.IntValue = 1;
        }
    }

    private GenericSetting CreateNamedIntData(string name)
    {
        var setting = new GenericSetting(name, GenericSetting.DataType.Integer);
        NamedIntDatas.Add(setting);
        return setting;
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        Settings.Clear();
        NamedIntDatas.Clear();
        PrefillNamedIntDatas(_gameData.NamedIntKeys);
        var saveDataWrapper =
            message.SelectedSavegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData);
        if (saveDataWrapper?.GetJsonBasedToken(Constants.JsonKeys.GameState) is not { } gameState)
        {
            return;
        }

        LoadSettings(gameState);
    }

    private void LoadSettings(JToken gameState)
    {
        var children = gameState.Children();
        foreach (var child in children.OfType<JProperty>())
        {
            GenericSetting? setting = null;
            switch (child.Name)
            {
                case "CrashSite":
                    var selectedItem = child.Value.ToString();
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.Enum, child.Path)
                    {
                        StringValue = selectedItem,
                        SelectedItem = selectedItem,
                        PossibleValues =
                        {
                            { "tree", "tree" },
                            { "ocean", "ocean" },
                            { "snow", "snow" }
                        }
                    };
                    break;
                case "SaveTime":
                case "GameType":
                case "IsVirginiaDead":
                case "IsRobbyDead":
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.ReadOnly, child.Path)
                    {
                        StringValue = child.Value.ToString()
                    };
                    break;
                case "GameName":
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.String, child.Path)
                    {
                        StringValue = child.Value.ToString()
                    };
                    break;
                case "GameDays":
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.Integer, child.Path)
                    {
                        IntValue = child.Value.Value<int>(),
                        MinInt = 0,
                        MaxInt = 999999
                    };
                    break;
                case "GameHours":
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.Integer, child.Path)
                    {
                        IntValue = child.Value.Value<int>(),
                        MinInt = 0,
                        MaxInt = 23
                    };
                    break;
                case "GameMinutes":
                case "GameSeconds":
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.Integer, child.Path)
                    {
                        IntValue = child.Value.Value<int>(),
                        MinInt = 0,
                        MaxInt = 59
                    };
                    break;
                case "GameMilliseconds":
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.Integer, child.Path)
                    {
                        IntValue = child.Value.Value<int>(),
                        MinInt = 0,
                        MaxInt = 999
                    };
                    break;
            }

            if (setting != null)
            {
                Settings.Add(setting);
            }
        }

        LoadNamedIntDatas(gameState);
    }

    private void LoadNamedIntDatas(JToken gameState)
    {
        if (gameState["NamedIntDatas"] is not { } namedIntDatas)
        {
            return;
        }

        var storedNamedIntSettings = new List<GenericSetting>();

        foreach (var namedIntData in namedIntDatas)
        {
            var nameId = namedIntData["SaveObjectNameId"]?.Value<string>();
            var value = namedIntData["SaveValue"]?.Value<int>();
            if (nameId == null || value is not { } saveValue)
            {
                continue;
            }

            if (saveValue > 3)
            {
                Logger.Warn($"SaveValue for {nameId} is > 3 ({value}), please report");
            }

            storedNamedIntSettings.Add(new GenericSetting(nameId, GenericSetting.DataType.Integer, namedIntData.Path)
            {
                IntValue = value,
                MaxInt = saveValue > 3 ? saveValue : 3
            });
        }

        var namedIntDatasByName = NamedIntDatas.ToDictionary(setting => setting.Name);

        foreach (var storedNamedIntSetting in storedNamedIntSettings)
        {
            if (!namedIntDatasByName.ContainsKey(storedNamedIntSetting.Name))
            {
                Logger.Info($"New NamedIntData found: {storedNamedIntSetting.Name}");
                NamedIntDatas.Add(storedNamedIntSetting);
            }
            else
            {
                namedIntDatasByName[storedNamedIntSetting.Name].IntValue = storedNamedIntSetting.IntValue;
            }
        }
    }

    public bool Update(Savegame savegame)
    {
        var saveDataWrapper =
            savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData);
        if (saveDataWrapper?.GetJsonBasedToken(Constants.JsonKeys.GameState) is not { } gameState)
        {
            return false;
        }

        if (!Merge(gameState, Settings, NamedIntDatas))
        {
            return false;
        }

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.GameState);
        return true;
    }

    private static bool Merge(JToken gameState, IEnumerable<GenericSetting> newSettings,
        IEnumerable<GenericSetting> newNamedIntDatas)
    {
        var hasChanges = false;
        hasChanges = newSettings.Aggregate(false, (current, setting) => setting.MergeTo(gameState) || current) ||
                     hasChanges;

        if (gameState["NamedIntDatas"] is { } namedIntDataToken)
        {
            hasChanges = MergeNamedIntDatas(newNamedIntDatas, namedIntDataToken) || hasChanges;
        }

        return hasChanges;
    }

    private static bool MergeNamedIntDatas(IEnumerable<GenericSetting> newNamedIntDatas, JToken namedIntDataToken)
    {
        var existingNamedIntData = namedIntDataToken.ToObject<List<NamedIntData>>() ?? Enumerable.Empty<NamedIntData>()
            .ToList();
        var newNamedIntData = newNamedIntDatas.Where(setting => setting.IntValue != null)
            .Select(setting => new NamedIntData(setting.Name, setting.IntValue!.Value))
            .ToList();

        if (newNamedIntData.SequenceEqual(existingNamedIntData))
        {
            return false;
        }

        namedIntDataToken.Replace(JToken.FromObject(newNamedIntData));

        return true;
    }
}