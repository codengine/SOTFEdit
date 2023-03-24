using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;

namespace SOTFEdit.ViewModel;

public partial class GameStatePageViewModel : ObservableObject
{
    private readonly Regex _resetCrateNameIdPattern =
        new(@"(\..*Crate.*\.)|(\..*Storage.*\.)|(\..*Case.*\.)|(.*\.Meds\..*)");

    public GameStatePageViewModel()
    {
        SetupListeners();
    }

    public ObservableCollection<GenericSetting> Settings { get; } = new();

    [NotifyCanExecuteChangedFor(nameof(ResetContainersCommand))] [ObservableProperty]
    private Savegame? _selectedSavegame;

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        SelectedSavegame = message.SelectedSavegame;
        Settings.Clear();
        var gameStateData =
            message.SelectedSavegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData);
        var gameStateToken = gameStateData?.SelectToken("Data.GameState");
        if (gameStateToken?.ToObject<string>() is not { } gameStateJson ||
            JsonConverter.DeserializeRaw(gameStateJson) is not { } gameState)
        {
            return;
        }

        LoadSettings(gameState);
    }

    private bool HasSavegame()
    {
        return _selectedSavegame != null;
    }

    [RelayCommand(CanExecute = nameof(HasSavegame))]
    public void ResetContainers()
    {
        if (SelectedSavegame is not { } savegame)
        {
            return;
        }

        var gameStateData = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData);

        if (gameStateData?.SelectToken("Data.GameState") is not { } gameStateToken ||
            gameStateToken.ToObject<string>() is not { } gameStateJson ||
            JsonConverter.DeserializeRaw(gameStateJson) is not { } gameState ||
            gameState["NamedIntDatas"] is not { } namedIntDatas)
        {
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent("Nothing to be reset", false));
            return;
        }

        var countFixed = 0;

        foreach (var data in namedIntDatas)
        {
            if (data["SaveObjectNameId"]?.ToObject<string>() is not { } nameId ||
                !_resetCrateNameIdPattern.Match(nameId).Success || data["SaveValue"] is not { } saveValueToken)
            {
                continue;
            }

            var oldSaveValue = saveValueToken.ToObject<int>();

            if (oldSaveValue != 1)
            {
                continue;
            }

            saveValueToken.Replace(0);
            countFixed++;
        }

        if (countFixed == 0)
        {
            WeakReferenceMessenger.Default.Send(new SavegameStoredEvent("Nothing to be reset", false));
            return;
        }

        gameStateToken.Replace(JsonConverter.Serialize(gameState));

        var createBackups = Ioc.Default.GetRequiredService<MainViewModel>().BackupFiles;
        savegame.SavegameStore.StoreJson(SavegameStore.FileType.GameStateSaveData, gameStateData, createBackups);

        WeakReferenceMessenger.Default.Send(
            new SavegameStoredEvent($"Reset {countFixed} containers, creates and pickups", true));
    }

    private void LoadSettings(JToken gameState)
    {
        var children = gameState.Children();
        foreach (var child in children.OfType<JProperty>())
        {
            GenericSetting? setting = null;
            switch (child.Name)
            {
                case "SaveTime":
                case "GameType":
                case "CrashSite":
                case "IsVirginiaDead":
                case "IsRobbyDead":
                    setting = new GenericSetting(child.Name, child.Path, GenericSetting.DataType.ReadOnly)
                    {
                        StringValue = child.Value.ToObject<string>()
                    };
                    break;
                case "GameDays":
                    setting = new GenericSetting(child.Name, child.Path, GenericSetting.DataType.Integer)
                    {
                        IntValue = child.Value.ToObject<int>(),
                        MinInt = 0,
                        MaxInt = 999999
                    };
                    break;
                case "GameHours":
                    setting = new GenericSetting(child.Name, child.Path, GenericSetting.DataType.Integer)
                    {
                        IntValue = child.Value.ToObject<int>(),
                        MinInt = 0,
                        MaxInt = 23
                    };
                    break;
                case "GameMinutes":
                case "GameSeconds":
                    setting = new GenericSetting(child.Name, child.Path, GenericSetting.DataType.Integer)
                    {
                        IntValue = child.Value.ToObject<int>(),
                        MinInt = 0,
                        MaxInt = 59
                    };
                    break;
                case "GameMilliseconds":
                    setting = new GenericSetting(child.Name, child.Path, GenericSetting.DataType.Integer)
                    {
                        IntValue = child.Value.ToObject<int>(),
                        MinInt = 0,
                        MaxInt = 999
                    };
                    break;
                case "CoreGameCompleted":
                case "EscapedIsland":
                case "StayedOnIsland":
                    setting = new GenericSetting(child.Name, child.Path, GenericSetting.DataType.Boolean)
                    {
                        BoolValue = child.Value.ToObject<bool>()
                    };
                    break;
            }

            if (setting != null)
            {
                Settings.Add(setting);
            }
        }
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        var gameStateData = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.GameStateSaveData);

        if (gameStateData?.SelectToken("Data.GameState") is not { } gameStateToken ||
            gameStateToken.ToObject<string>() is not { } gameStateJson ||
            JsonConverter.DeserializeRaw(gameStateJson) is not { } gameState)
        {
            return false;
        }

        if (!Merge(gameState, Settings))
        {
            return false;
        }

        gameStateToken.Replace(JsonConverter.Serialize(gameState));

        savegame.SavegameStore.StoreJson(SavegameStore.FileType.GameStateSaveData, gameStateData, createBackup);
        return true;
    }

    private static bool Merge(JToken gameState, IEnumerable<GenericSetting> settings)
    {
        return settings.Aggregate(false, (current, setting) => setting.MergeTo(gameState) || current);
    }
}