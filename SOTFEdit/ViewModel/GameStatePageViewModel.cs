using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData;

namespace SOTFEdit.ViewModel;

public partial class GameStatePageViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly GameData _gameData;

    [ObservableProperty] private Savegame? _selectedSavegame;

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
            NamedIntDatas.Add(new GenericSetting(namedIntKey, GenericSetting.DataType.Integer)
            {
                IntValue = null
            });
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        SelectedSavegame = message.SelectedSavegame;
        Settings.Clear();
        NamedIntDatas.Clear();
        PrefillNamedIntDatas(_gameData.NamedIntKeys);
        var gameStateData =
            message.SelectedSavegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData);
        var gameStateToken = gameStateData?.SelectToken("Data.GameState");
        if (gameStateToken?.ToString() is not { } gameStateJson ||
            JsonConverter.DeserializeRaw(gameStateJson) is not { } gameState)
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
                case "CoreGameCompleted":
                case "EscapedIsland":
                case "StayedOnIsland":
                    setting = new GenericSetting(child.Name, GenericSetting.DataType.Boolean, child.Path)
                    {
                        BoolValue = child.Value.Value<bool>()
                    };
                    break;
            }

            if (setting != null)
            {
                Settings.Add(setting);
            }
        }

        if (gameState["NamedIntDatas"] is { } namedIntDatas)
        {
            var storedNamedIntSettings = new List<GenericSetting>();

            foreach (var namedIntData in namedIntDatas)
            {
                var nameId = namedIntData["SaveObjectNameId"]?.Value<string>();
                var value = namedIntData["SaveValue"]?.Value<int>();
                if (nameId != null && value != null)
                {
                    storedNamedIntSettings.Add(new GenericSetting(nameId, GenericSetting.DataType.Integer, namedIntData.Path)
                    {
                        IntValue = value
                    });
                }
            }

            var namedIntDatasByName = NamedIntDatas.ToDictionary(setting => setting.Name);

            foreach (var storedNamedIntSetting in storedNamedIntSettings)
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

    public bool Update(Savegame savegame, bool createBackup)
    {
        var gameStateData = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData);

        if (gameStateData?.SelectToken("Data.GameState") is not { } gameStateToken ||
            gameStateToken.ToString() is not { } gameStateJson ||
            JsonConverter.DeserializeRaw(gameStateJson) is not { } gameState)
        {
            return false;
        }

        if (!Merge(gameState, Settings, NamedIntDatas))
        {
            return false;
        }

        gameStateToken.Replace(JsonConverter.Serialize(gameState));

        savegame.SavegameStore.StoreJson(SavegameStore.FileType.GameStateSaveData, gameStateData, createBackup);
        return true;
    }

    private static bool Merge(JToken gameState, IEnumerable<GenericSetting> newSettings, IEnumerable<GenericSetting> newNamedIntDatas)
    {
        var hasChanges = false;
        hasChanges = newSettings.Aggregate(false, (current, setting) => setting.MergeTo(gameState) || current) || hasChanges;

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