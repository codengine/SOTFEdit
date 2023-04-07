using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Actor;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Actor;
using static SOTFEdit.Model.Constants.Actors;

namespace SOTFEdit.ViewModel;

public partial class FollowerPageViewModel : ObservableObject
{
    private const int TeleportYoffset = 1;
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReviveCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToKelvinCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToVirginiaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToPlayerCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpawnCommand))]
    private Savegame? _selectedSavegame;

    public FollowerPageViewModel(GameData gameData)
    {
        SetupListeners();

        KelvinState = new KelvinState(gameData.FollowerData.GetOutfits(KelvinTypeId),
            gameData.FollowerData.GetEquippableItems(KelvinTypeId, gameData.Items));
        VirginiaState = new VirginiaState(gameData.FollowerData.GetOutfits(VirginiaTypeId),
            gameData.FollowerData.GetEquippableItems(VirginiaTypeId, gameData.Items));
    }

    public KelvinState KelvinState { get; }
    public VirginiaState VirginiaState { get; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent m)
    {
        SelectedSavegame = m.SelectedSavegame;
        KelvinState.Reset();
        VirginiaState.Reset();

        LoadFollowerData(m);
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToPlayer(FollowerState follower)
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        var playerPos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos;
        follower.Pos = playerPos with { Y = playerPos.Y + TeleportYoffset };
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToKelvin(FollowerState follower)
    {
        var kelvinPos = KelvinState.Pos;
        follower.Pos = kelvinPos with { Y = kelvinPos.Y + TeleportYoffset };
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToVirginia(FollowerState follower)
    {
        var virginiaPos = VirginiaState.Pos;
        follower.Pos = virginiaPos with { Y = virginiaPos.Y + TeleportYoffset };
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void Revive(FollowerState follower)
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        var backupFiles = Ioc.Default.GetRequiredService<MainViewModel>().BackupFiles;
        var pos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos;
        WeakReferenceMessenger.Default.Send(new RequestReviveFollowersEvent(SelectedSavegame, backupFiles,
            follower.TypeId, follower.GetSelectedInventoryItemIds(), follower.Outfit, pos));
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void Spawn(FollowerState follower)
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        var playerPos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos;
        var createBackup = Ioc.Default.GetRequiredService<MainViewModel>().BackupFiles;

        WeakReferenceMessenger.Default.Send(new RequestSpawnFollowerEvent(SelectedSavegame,
            follower.TypeId, follower.GetSelectedInventoryItemIds(), follower.Outfit, playerPos, createBackup));
    }

    public bool CanSaveChanges()
    {
        return SelectedSavegame != null;
    }

    private void LoadFollowerData(SelectedSavegameChangedEvent m)
    {
        if (m.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var saveData = selectedSavegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData);

        var vailWorldSimToken = saveData?.SelectToken("Data.VailWorldSim");
        if (vailWorldSimToken?.ToString() is not { } vailWorldSimJson ||
            JsonConverter.DeserializeRaw(vailWorldSimJson) is not JObject vailWorldSim)
        {
            return;
        }

        foreach (var actor in vailWorldSim["Actors"] ?? Enumerable.Empty<JToken>())
        {
            var typeId = actor["TypeId"]?.Value<int>();
            if (typeId is not (KelvinTypeId or VirginiaTypeId))
            {
                continue;
            }

            FollowerState followerModel = typeId == KelvinTypeId ? KelvinState : VirginiaState;

            if (actor["UniqueId"]?.Value<int>() is { } uniqueId)
            {
                followerModel.UniqueId = uniqueId;
            }

            followerModel.Status = actor["State"]?.Value<int>() switch
            {
                2 => "Alive",
                6 => "Dead",
                _ => "???"
            };

            if (actor["Position"] is { } position)
            {
                followerModel.Pos = position.ToObject<Position>() ?? new Position(0, 0, 0);
            }

            if (actor["Stats"] is { } stats)
            {
                followerModel.Health = stats["Health"]?.Value<float>() ?? 0f;
                followerModel.Hydration = stats["Hydration"]?.Value<float>() ?? 0f;
                followerModel.Energy = stats["Energy"]?.Value<float>() ?? 0f;

                switch (followerModel)
                {
                    case KelvinState kelvinState:
                        kelvinState.Fear = stats["Fear"]?.Value<float>() ?? 0f;
                        break;
                    case VirginiaState virginiaState:
                        virginiaState.Affection = stats["Affection"]?.Value<float>() ?? 0f;
                        break;
                }
            }

            if (actor["EquippedItems"] is JArray equippedItems && equippedItems.ToObject<int[]>() is { } itemIds)
            {
                foreach (var itemId in itemIds)
                {
                    if (followerModel.Inventory.FirstOrDefault(item => item.ItemId == itemId) is not { } inventoryItem)
                    {
                        Logger.Warn($"Unknown item found in inventory for follower {typeId}: {itemId}");
                        var item = new Item
                        {
                            Id = itemId,
                            Name = "???"
                        };
                        followerModel.Inventory.Add(new EquippableItem(item, true)
                        {
                            Selected = true
                        });
                        continue;
                    }

                    inventoryItem.Selected = true;
                }
            }

            if (actor["OutfitId"]?.Value<int>() is { } outfitId)
            {
                followerModel.Outfit = followerModel.Outfits.FirstOrDefault(outfit => outfit.Id == outfitId);
            }
            else
            {
                followerModel.Outfit = followerModel.Outfits.FirstOrDefault(outfit => outfit.Id == 0);
            }
        }

        foreach (var influenceMemory in vailWorldSim["InfluenceMemory"] ?? Enumerable.Empty<JToken>())
        {
            if (influenceMemory["UniqueId"]?.Value<int>() is not { } uniqueId)
            {
                continue;
            }

            FollowerState followerState;

            if (uniqueId == KelvinState.UniqueId)
            {
                followerState = KelvinState;
            }
            else if (uniqueId == VirginiaState.UniqueId)
            {
                followerState = VirginiaState;
            }
            else
            {
                continue;
            }

            foreach (var influence in influenceMemory["Influences"]?.ToObject<List<Influence>>() ??
                                      Enumerable.Empty<Influence>())
                followerState.Influences.Add(influence);
        }
    }

    public bool Update(Savegame? savegame, bool createBackup)
    {
        if (savegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not JObject saveData)
        {
            return false;
        }

        var saveDataWrapper = new SaveDataWrapper(saveData);
        var followerModifier = new FollowerModifier(saveDataWrapper);
        var hasChanges = followerModifier.Update(new FollowerState[] { KelvinState, VirginiaState }) &&
                         saveDataWrapper.SerializeAllModified();

        if (!hasChanges)
        {
            return false;
        }

        savegame.SavegameStore.StoreJson(SavegameStore.FileType.SaveData, saveData, createBackup);
        return true;
    }
}