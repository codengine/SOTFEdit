using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Infrastructure;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;
using SOTFEdit.Model.Map.Static;
using SOTFEdit.Model.Savegame;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly ArmorPageViewModel _armorPageViewModel;
    private readonly CompanionConnectionManager _companionConnectionManager;
    private readonly MapManager _mapManager;
    private readonly CompanionNetworkPlayerManager _networkPlayerManager;
    private readonly PoiLoader _poiLoader;

    [ObservableProperty]
    private bool _checkVersionOnStartup;

    [ObservableProperty]
    private bool _isMapWindowClosed = true;

    [ObservableProperty]
    private string _lastSaveGameMenuItem = TranslationManager.Get("menu.openLastSavegame");

    [ObservableProperty]
    private double _pinLeft;

    [ObservableProperty]
    private Position? _pinPos;

    [ObservableProperty]
    private double _pinTop;

    public MainViewModel(ArmorPageViewModel armorPageViewModel, GamePage gamePage, NpcsPage npcsPage,
        StructuresPage structuresPage, MapManager mapManager, PoiLoader poiLoader,
        CompanionNetworkPlayerManager networkPlayerManager, CompanionConnectionManager companionConnectionManager)
    {
        _armorPageViewModel = armorPageViewModel;
        _mapManager = mapManager;
        _poiLoader = poiLoader;
        _networkPlayerManager = networkPlayerManager;
        _companionConnectionManager = companionConnectionManager;
        GamePage = gamePage;
        NpcsPage = npcsPage;
        StructuresPage = structuresPage;
        CheckVersionOnStartup = Settings.Default.CheckForUpdates;

        UpdateLastSaveGameMenuItem();

        SetupListeners();
    }

    public GamePage GamePage { get; }
    public InventoryPage InventoryPage { get; } = new();
    public FollowersPage FollowersPage { get; } = new();
    public PlayerPage PlayerPage { get; } = new();
    public StoragePage StoragePage { get; } = new();
    public NpcsPage NpcsPage { get; }
    public StructuresPage StructuresPage { get; }

    public object? SelectedTab { get; set; }

    public bool CanEditTabs => CanSaveAndEdit();

    public string CompanionConnectMenuText
    {
        get
        {
            return _companionConnectionManager.Status switch
            {
                CompanionConnectionManager.ConnectionStatus.Connected => TranslationManager.Get("companion.disconnect"),
                CompanionConnectionManager.ConnectionStatus.Connecting => TranslationManager.Get(
                    "companion.status.connecting"),
                CompanionConnectionManager.ConnectionStatus.Disconnected => TranslationManager.Get("companion.connect"),
                _ => _companionConnectionManager.Status.ToString()
            };
        }
    }

    partial void OnIsMapWindowClosedChanged(bool value)
    {
        RefreshCanExecuteChanged();
    }

    partial void OnCheckVersionOnStartupChanged(bool value)
    {
        Settings.Default.CheckForUpdates = value;
        Settings.Default.Save();
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void ReloadSavegame()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var replacementSavegame = SavegameManager.ReloadSavegame(selectedSavegame);
        SavegameManager.SelectedSavegame = replacementSavegame;
    }

    [RelayCommand]
    private static void SelectSavegame()
    {
        WeakReferenceMessenger.Default.Send(new RequestSelectSavegameEvent());
    }

    [RelayCommand]
    private static void ExitApplication()
    {
        WeakReferenceMessenger.Default.Send(new RequestApplicationExitEvent());
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SavegameStoredEvent>(this,
            (_, _) => ReloadSavegame());

        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, message) => OnSelectedSavegameChanged(message));

        WeakReferenceMessenger.Default.Register<CompanionConnectionStatusEvent>(this, (_, _) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CompanionConnectCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CompanionConnectMenuText));
            });
        });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent message)
    {
        RefreshCanExecuteChanged();
        Settings.Default.LastSavegame = message.SelectedSavegame?.FullPath ?? "";
        Settings.Default.LastSavegameName = message.SelectedSavegame?.GameName ?? "";
        Settings.Default.Save();
        UpdateLastSaveGameMenuItem();
        OpenLastSavegameCommand.NotifyCanExecuteChanged();
    }

    private void RefreshCanExecuteChanged()
    {
        SaveChangesCommand.NotifyCanExecuteChanged();
        ReloadSavegameCommand.NotifyCanExecuteChanged();
        OpenSavegameDirCommand.NotifyCanExecuteChanged();
        DeleteBackupsCommand.NotifyCanExecuteChanged();
        ResetCannibalAngerCommand.NotifyCanExecuteChanged();
        RestoreBackupsCommand.NotifyCanExecuteChanged();
        RegrowTreesCommand.NotifyCanExecuteChanged();
        ModifyConsumedItemsCommand.NotifyCanExecuteChanged();
        EternalFiresCommand.NotifyCanExecuteChanged();
        ResetFiresCommand.NotifyCanExecuteChanged();
        ResetStructureDamageCommand.NotifyCanExecuteChanged();
        TeleportWorldItemCommand.NotifyCanExecuteChanged();
        ResetContainersCommand.NotifyCanExecuteChanged();
        ResetTrapsCommand.NotifyCanExecuteChanged();
        SetToEndgameCommand.NotifyCanExecuteChanged();
        ItemPlaterCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanEditTabs));
    }

    private void UpdateLastSaveGameMenuItem()
    {
        if (Settings.Default.LastSavegame is not { } lastSavegame)
        {
            LastSaveGameMenuItem = TranslationManager.Get("menu.openLastSavegame");
            return;
        }

        var fnDisplay = lastSavegame;

        var parts = lastSavegame.Split(Path.DirectorySeparatorChar);
        if (parts.Length >= 2)
        {
            fnDisplay = $"{parts[^2]}{Path.DirectorySeparatorChar}{parts[^1]}";
        }

        if (!string.IsNullOrWhiteSpace(Settings.Default.LastSavegameName))
        {
            fnDisplay = $"{Settings.Default.LastSavegameName} ({fnDisplay})";
        }

        LastSaveGameMenuItem = TranslationManager.GetFormatted("menu.file.openLastSavegameWithSavegame", fnDisplay);
    }

    public static bool CanOpenLastSavegame()
    {
        return !string.IsNullOrEmpty(Settings.Default.LastSavegame) && Directory.Exists(Settings.Default.LastSavegame);
    }

    [RelayCommand(CanExecute = nameof(CanOpenLastSavegame))]
    private void OpenLastSavegame()
    {
        if (!CanOpenLastSavegame())
        {
            WeakReferenceMessenger.Default.Send(
                new GenericMessageEvent("Unable to open the last savegame. Has it been deleted?",
                    TranslationManager.Get("generic.error")));
        }

        var directoryInfo = new DirectoryInfo(Settings.Default.LastSavegame);
        SavegameManager.SelectedSavegame = SavegameManager.CreateSaveInfo(directoryInfo);
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private void SaveChanges()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(selectedSavegame, backupMode =>
        {
            var hasChanges = Ioc.Default.GetRequiredService<GamePage>().Update(selectedSavegame);
            hasChanges = InventoryPage.Update(selectedSavegame) || hasChanges;
            hasChanges = _armorPageViewModel.Update(selectedSavegame) || hasChanges;
            hasChanges = FollowersPage.Update(selectedSavegame) || hasChanges;
            hasChanges = PlayerPage.Update(selectedSavegame) || hasChanges;
            hasChanges = StoragePage.Update(selectedSavegame) || hasChanges;
            hasChanges = StructuresPage.Update(selectedSavegame) || hasChanges;

            hasChanges = selectedSavegame.SavegameStore.SaveAllModified(backupMode) || hasChanges;

            if (hasChanges)
            {
                WeakReferenceMessenger.Default.Send(
                    new SavegameStoredEvent(TranslationManager.Get("windows.main.messages.changesSaved")));
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                    TranslationManager.Get("windows.main.messages.noChanges.text"),
                    TranslationManager.Get("windows.main.messages.noChanges.title")));
            }
        }));
    }

    private bool CanSaveAndEdit()
    {
        return IsSavegameSelected() && IsMapWindowClosed;
    }

    private static bool IsSavegameSelected()
    {
        return SavegameManager.SelectedSavegame != null;
    }

    [RelayCommand]
    private static void OpenUrl(string url)
    {
        WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForUrl(url));
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void OpenSavegameDir()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForExplorer(selectedSavegame.FullPath));
    }

    [RelayCommand(CanExecute = nameof(IsSavegameSelected))]
    private void DeleteBackups()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestDeleteBackupsEvent(selectedSavegame));
    }

    [RelayCommand]
    private static void CheckForUpdates()
    {
        WeakReferenceMessenger.Default.Send(new RequestCheckForUpdatesEvent(true, true));
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private void ResetCannibalAnger()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        try
        {
            LabExperiments.ResetCannibalAnger(selectedSavegame);
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.Get("experiments.resetCannibalAngerLevel.success.text"),
                TranslationManager.Get("experiments.resetCannibalAngerLevel.success.title")));
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.GetFormatted("generic.exceptionMessage", ex.Message),
                TranslationManager.Get("generic.error")));
        }
    }

    private bool CompanionCanConnect()
    {
        return _companionConnectionManager.Status != CompanionConnectionManager.ConnectionStatus.Connecting;
    }

    [RelayCommand(CanExecute = nameof(CompanionCanConnect))]
    private async Task CompanionConnect()
    {
        if (!_companionConnectionManager.IsConnected())
        {
            await DoConnect();
        }
        else
        {
            await DoDisconnect();
        }
    }

    private async Task DoConnect()
    {
        try
        {
            var task = _companionConnectionManager.ConnectAsync();
            var connected = await task;
            if (!connected)
            {
                WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                    TranslationManager.Get("companion.connectionFailed.text"),
                    TranslationManager.Get("companion.connectionFailed.title")));
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                    TranslationManager.Get("companion.connectionSuccessful.text"),
                    TranslationManager.Get("companion.connectionSuccessful.title")));
            }
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                string.Format(TranslationManager.Get("companion.connectionFailed.text"), ex.Message),
                TranslationManager.Get("companion.connectionFailed.title")));
        }
        finally
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CompanionConnectCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CompanionConnectMenuText));
            });
        }
    }

    private async Task DoDisconnect()
    {
        try
        {
            await _companionConnectionManager.DisconnectAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                string.Format(TranslationManager.Get("companion.connectionFailed.text"), ex.Message),
                TranslationManager.Get("companion.connectionFailed.title")));
        }
        finally
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CompanionConnectCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CompanionConnectMenuText));
            });
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private void ModifyConsumedItems()
    {
        WeakReferenceMessenger.Default.Send(new RequestModifyConsumedItemsEvent());
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private void RestoreBackups(string restoreFromNewest)
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new RequestRestoreBackupsEvent(selectedSavegame,
            bool.Parse(restoreFromNewest)));
    }

    [RelayCommand]
    private static void ChangeSettings()
    {
        WeakReferenceMessenger.Default.Send(new RequestChangeSettingsEvent());
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private void RegrowTrees()
    {
        WeakReferenceMessenger.Default.Send(new RequestRegrowTreesEvent());
    }

    [RelayCommand]
    private static void Unlocks()
    {
        WeakReferenceMessenger.Default.Send(new RequestChangeUnlocksEvent());
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private static void SetToEndgame()
    {
        WeakReferenceMessenger.Default.Send(new RequestSetToEndgameEvent());
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private static void ItemPlater()
    {
        WeakReferenceMessenger.Default.Send(new RequestItemPlaterEvent());
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private static void ResetContainers()
    {
        WeakReferenceMessenger.Default.Send(new RequestResetContainersEvent());
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private static void ResetTraps()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame ||
            selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.ScrewTrapsSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.ScrewTraps) is not { } traps ||
            traps["_tarpStructureData"] is not JArray structureData || structureData.Count == 0)
        {
            ShowNoTrapsToResetMessage();
            return;
        }

        var countReset = 0;

        foreach (var trapData in structureData)
        {
            if (trapData["IsTriggered"] is { } isTriggeredToken && isTriggeredToken.Value<bool>())
            {
                trapData["IsTriggered"] = false;
                countReset++;
            }

            if (trapData["Data1"] is { } data1Token && data1Token.Value<int>() < 5)
            {
                trapData["Data1"] = 5;
                countReset++;
            }
        }

        if (countReset == 0)
        {
            ShowNoTrapsToResetMessage();
            return;
        }

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.ScrewTraps);
        WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
            TranslationManager.GetFormatted("windows.resetTraps.messages.trapsReset.text", countReset),
            TranslationManager.Get("windows.resetTraps.messages.trapsReset.title")));
    }

    private static void ShowNoTrapsToResetMessage()
    {
        WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
            TranslationManager.Get("windows.resetTraps.messages.nothingToReset.text"),
            TranslationManager.Get("windows.resetTraps.messages.nothingToReset.title")));
    }

    public void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F5 && ReloadSavegameCommand.CanExecute(null))
        {
            e.Handled = true;
            ReloadSavegameCommand.Execute(null);
        }

        if (e.Key != Key.S || Keyboard.Modifiers != ModifierKeys.Control || !SaveChangesCommand.CanExecute(null))
        {
            return;
        }

        e.Handled = true;
        SaveChangesCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private void ResetStructureDamage()
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame ||
            selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.StructureDestructionSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.StructureDestruction) is not
                { } structureDestruction ||
            structureDestruction["Data"] is not JArray destructionData)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.Get("experiments.resetStructureDamage.noDamage.text"),
                TranslationManager.Get("experiments.resetStructureDamage.noDamage.title")));
            return;
        }

        var entryCount = destructionData.Count;
        destructionData.Clear();
        saveDataWrapper.MarkAsModified(Constants.JsonKeys.StructureDestruction);

        WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
            TranslationManager.GetFormatted("experiments.resetStructureDamage.success.text", entryCount),
            TranslationManager.Get("experiments.resetStructureDamage.success.title")));
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private void EternalFires()
    {
        _changeFires(true, 65535f, 0.00001f);
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private void ResetFires()
    {
        _changeFires(true, 300f, 1.0f);
    }

    private static void _changeFires(bool isLit, float fuel, float fuelDrainRate)
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame ||
            selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.FiresSaveData) is not
                { } saveDataWrapper ||
            saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.Fires) is not { } fires ||
            fires["FiresPerStructureType"] is not { } firesPerStructureType)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                TranslationManager.Get("experiments.fires.noFires.text"),
                TranslationManager.Get("experiments.fires.noFires.title")));
            return;
        }

        var countChanged = 0;

        foreach (var fireType in firesPerStructureType)
        foreach (var firesByType in fireType.Children())
        foreach (var fire in firesByType)
        {
            fire["IsLit"] = isLit;
            fire["Fuel"] = fuel;
            fire["FuelDrainRate"] = fuelDrainRate;
            countChanged++;
        }

        saveDataWrapper.MarkAsModified(Constants.JsonKeys.Fires);
        WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
            TranslationManager.GetFormatted("experiments.fires.success.text", countChanged),
            TranslationManager.Get("experiments.fires.success.title")));
    }

    [RelayCommand(CanExecute = nameof(CanSaveAndEdit))]
    private void TeleportWorldItem()
    {
        WeakReferenceMessenger.Default.Send(new RequestTeleportWorldItemEvent());
    }

    [RelayCommand]
    private void OpenMap()
    {
        var actorPoiGroups = _mapManager.GetActorPois();

        var structurePoiGroups = _mapManager.GetStructurePois()
            .Select(kvp => new PoiGroup(false, kvp.Value, kvp.Key,
                PoiGroupKeys.Structures +
                (kvp.Value.FirstOrDefault()?.ScrewStructureWrapper.ScrewStructure?.Id.ToString() ?? ""),
                PoiGroupType.Structures, kvp.Value.First().Icon))
            .ToList();

        var poiGroups = new List<IPoiGrouper>
        {
            PoiGroupCollection.ForActors(actorPoiGroups),
            new PoiGroupCollection(false, TranslationManager.Get("map.structures"), PoiGroupKeys.Structures,
                structurePoiGroups,
                PoiGroupType.Structures)
        };

        if (SavegameManager.SelectedSavegame is { } selectedSavegame)
        {
            poiGroups.AddRange(MapManager.GetWorldItemPois(selectedSavegame)
                .Select(kvp =>
                    new PoiGroup(false, kvp.Value, kvp.Key, PoiGroupKeys.WorldItems + kvp.Key, PoiGroupType.WorldItems,
                        kvp.Value.First().IconSmall)));

            var ziplinePois = MapManager.GetZiplinePois(selectedSavegame);

            if (ziplinePois.Count > 0)
            {
                var zipPois = new List<IPoi>();

                foreach (var ziplinePoi in ziplinePois)
                {
                    zipPois.Add(ziplinePoi.PointA);
                    zipPois.Add(ziplinePoi.PointB);
                    zipPois.Add(ziplinePoi);
                }

                poiGroups.Add(new PoiGroup(false, zipPois, TranslationManager.Get("map.zipLines"),
                    PoiGroupType.ZipLines,
                    ZipPointPoi.IconSmall));
            }
        }

        poiGroups.AddRange(_poiLoader.Load());

        var playerPageViewModel = Ioc.Default.GetRequiredService<PlayerPageViewModel>();
        var playerPos = playerPageViewModel.PlayerState.Pos;

        poiGroups.Add(new PoiGroup(true, BuildPlayerPois(playerPos, playerPageViewModel),
            TranslationManager.Get("player.mapGroupName"), PoiGroupType.Player, PlayerPoi.IconSmall));

        WeakReferenceMessenger.Default.Send(new RequestOpenMapEvent(poiGroups.OrderBy(group => group.Title).ToList()));
    }

    private IEnumerable<IPoi> BuildPlayerPois(Position playerPos, PlayerPageViewModel playerPageViewModel)
    {
        var playerPois = new List<IPoi>
        {
            new PlayerPoi(playerPos)
            {
                Enabled = true,
                Position = playerPageViewModel.PlayerState.Pos
            }
        };

        playerPois.AddRange(
            _networkPlayerManager.InstanceIds.Select(instanceId =>
            {
                var poi = new NetworkPlayerPoi(new Position(0, 0, 0), instanceId, "???");
                poi.SetEnabledNoRefresh(true);
                return poi;
            })
        );

        return playerPois;
    }

    [RelayCommand]
    private static void CompanionSetup()
    {
        WeakReferenceMessenger.Default.Send(new OpenCompanionSetupWindowEvent());
    }
}