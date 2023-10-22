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
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Actor;
using SOTFEdit.Model.Savegame;
using static SOTFEdit.Model.Constants.Actors;

namespace SOTFEdit.ViewModel;

public partial class FollowerPageViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public FollowerPageViewModel(GameData gameData)
    {
        SetupListeners();

        KelvinState = BuildFollowerState(gameData, KelvinTypeId);
        VirginiaState = BuildFollowerState(gameData, VirginiaTypeId);
        AllInfluences = Influence.AllTypes.Select(type =>
                new ComboBoxItemAndValue<string>(TranslationManager.Get("actors.influenceType." + type), type))
            .ToList();
    }

    public IEnumerable<ComboBoxItemAndValue<string>> AllInfluences { get; }

    public FollowerState KelvinState { get; }
    public FollowerState VirginiaState { get; }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void FillAllBars(FollowerState followerState)
    {
        followerState.Health = 100;
        followerState.Fullness = 100;
        followerState.Hydration = 100;
        followerState.Energy = 100;
        followerState.Affection = 100;
    }

    private static FollowerState BuildFollowerState(GameData gameData, int typeId)
    {
        return new FollowerState(
            typeId,
            gameData.FollowerData.GetOutfits(typeId),
            gameData.FollowerData.GetEquippableItems(typeId, gameData.Items)
        );
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => OnSelectedSavegameChanged(m));
        WeakReferenceMessenger.Default.Register<JsonModelChangedEvent>(this,
            (_, message) => OnJsonModelChangedEvent(message));
        WeakReferenceMessenger.Default.Register<PlayerPosChangedEvent>(this, (_, _) => OnPlayerPosChangedEvent());
    }

    private void OnPlayerPosChangedEvent()
    {
        MoveToPlayerCommand.NotifyCanExecuteChanged();
    }

    private void OnJsonModelChangedEvent(JsonModelChangedEvent message)
    {
        if (message.FileType != SavegameStore.FileType.SaveData)
        {
            return;
        }

        Reload(SavegameManager.SelectedSavegame);
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent m)
    {
        Reload(m.SelectedSavegame);
        ReviveCommand.NotifyCanExecuteChanged();
        MoveToPlayerCommand.NotifyCanExecuteChanged();
        MoveToKelvinCommand.NotifyCanExecuteChanged();
        MoveToVirginiaCommand.NotifyCanExecuteChanged();
        SpawnCommand.NotifyCanExecuteChanged();
        FillAllBarsCommand.NotifyCanExecuteChanged();
    }

    private void Reload(Savegame? selectedSavegame)
    {
        KelvinState.Reset();
        VirginiaState.Reset();

        if (selectedSavegame != null)
        {
            LoadFollowerData(selectedSavegame);
        }
    }

    private static bool CanMoveToPlayer()
    {
        return CanSaveChanges() &&
               Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos.Area.IsSurface();
    }

    [RelayCommand(CanExecute = nameof(CanMoveToPlayer))]
    private void MoveToPlayer(FollowerState follower)
    {
        if (SavegameManager.SelectedSavegame == null)
        {
            return;
        }

        var playerPos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos;
        follower.Pos = Teleporter.MoveToPos(playerPos);
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToKelvin(FollowerState follower)
    {
        follower.Pos = Teleporter.MoveToPos(KelvinState.Pos);
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void MoveToVirginia(FollowerState follower)
    {
        follower.Pos = Teleporter.MoveToPos(VirginiaState.Pos);
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void Revive(FollowerState follower)
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var pos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos;
        WeakReferenceMessenger.Default.Send(new RequestReviveFollowersEvent(selectedSavegame, follower.TypeId,
            follower.GetSelectedInventoryItemIds(), follower.Outfit, pos));
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void Spawn(FollowerState follower)
    {
        if (SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            return;
        }

        var playerPos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos;

        WeakReferenceMessenger.Default.Send(new RequestSpawnFollowerEvent(selectedSavegame,
            follower.TypeId, follower.GetSelectedInventoryItemIds(), follower.Outfit, playerPos));
    }

    public static bool CanSaveChanges()
    {
        return SavegameManager.SelectedSavegame != null;
    }

    private void LoadFollowerData(Savegame savegame)
    {
        var saveDataWrapper = savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData);

        if (saveDataWrapper?.GetJsonBasedToken(Constants.JsonKeys.VailWorldSim) is not JObject vailWorldSim)
        {
            return;
        }

        var foundVirginia = false;
        var foundKelvin = false;

        foreach (var actor in vailWorldSim["Actors"] ?? Enumerable.Empty<JToken>())
        {
            var typeId = actor["TypeId"]?.Value<int>();
            if (typeId is not (KelvinTypeId or VirginiaTypeId))
            {
                continue;
            }

            switch (typeId)
            {
                case KelvinTypeId when foundKelvin:
                case VirginiaTypeId when foundVirginia:
                    continue;
            }

            var followerModel = typeId == KelvinTypeId ? KelvinState : VirginiaState;

            switch (typeId)
            {
                case KelvinTypeId:
                    foundKelvin = true;
                    break;
                case VirginiaTypeId:
                    foundVirginia = true;
                    break;
            }

            if (actor["UniqueId"]?.Value<int>() is { } uniqueId)
            {
                followerModel.UniqueId = uniqueId;
            }

            followerModel.Status = actor["State"]?.Value<int>() switch
            {
                2 => TranslationManager.Get("followers.status.alive"),
                6 => TranslationManager.Get("followers.status.dead"),
                _ => TranslationManager.Get("followers.status.unknown")
            };

            if (actor["Position"] is { } position)
            {
                followerModel.Pos = position.ToObject<Position>() ?? new Position(0, 0, 0);
            }

            if (actor["Stats"] is { } stats)
            {
                followerModel.Health = stats["Health"]?.Value<float>() ?? 0f;
                followerModel.Anger = stats["Anger"]?.Value<float>() ?? 0f;
                followerModel.Fear = stats["Fear"]?.Value<float>() ?? 0f;
                followerModel.Fullness = stats["Fullness"]?.Value<float>() ?? 0f;
                followerModel.Hydration = stats["Hydration"]?.Value<float>() ?? 0f;
                followerModel.Energy = stats["Energy"]?.Value<float>() ?? 0f;
                followerModel.Affection = stats["Affection"]?.Value<float>() ?? 0f;
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
                            Id = itemId
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
            {
                followerState.Influences.Add(influence);
            }
        }
    }

    public bool Update(Savegame? savegame)
    {
        if (savegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not { } saveDataWrapper)
        {
            return false;
        }

        var followerModifier = new FollowerModifier(saveDataWrapper);
        return followerModifier.Update(new[] { KelvinState, VirginiaState });
    }

    [RelayCommand]
    private static void OpenMapAtFollower(Position pos)
    {
        WeakReferenceMessenger.Default.Send(new ZoomToPosEvent(pos));
    }
}