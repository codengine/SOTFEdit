using System;
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
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData;
using static SOTFEdit.Model.Constants.Actors;

namespace SOTFEdit.ViewModel;

public partial class FollowerPageViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReviveCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToKelvinCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToVirginiaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToPlayerCommand))]
    private Savegame? _selectedSavegame;

    public FollowerPageViewModel(GameData gameData)
    {
        SetupListeners();

        KelvinState = BuildFollowerState(gameData, KelvinTypeId);
        VirginiaState = BuildFollowerState(gameData, VirginiaTypeId);
    }

    public FollowerState KelvinState { get; set; }
    public FollowerState VirginiaState { get; }

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
    public void MoveToPlayer(FollowerState follower)
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        follower.Pos = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Pos.Copy();
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    public void MoveToKelvin(FollowerState follower)
    {
        follower.Pos = KelvinState.Pos.Copy();
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    public void MoveToVirginia(FollowerState follower)
    {
        follower.Pos = VirginiaState.Pos.Copy();
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    public void Revive(FollowerState follower)
    {
        if (SelectedSavegame == null)
        {
            return;
        }

        var backupFiles = Ioc.Default.GetRequiredService<MainViewModel>().BackupFiles;
        WeakReferenceMessenger.Default.Send(new RequestReviveFollowersEvent(SelectedSavegame, backupFiles,
            follower.TypeId));
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
        if (vailWorldSimToken?.ToObject<string>() is not { } vailWorldSimJson ||
            JsonConverter.DeserializeRaw(vailWorldSimJson) is not JObject vailWorldSim)
        {
            return;
        }

        foreach (var actor in vailWorldSim["Actors"] ?? Enumerable.Empty<JToken>())
        {
            var typeId = actor["TypeId"]?.ToObject<int>();
            if (typeId is not (KelvinTypeId or VirginiaTypeId))
            {
                continue;
            }

            var followerModel = typeId == KelvinTypeId ? KelvinState : VirginiaState;

            if (actor["UniqueId"]?.ToObject<int>() is { } uniqueId)
            {
                followerModel.UniqueId = uniqueId;
            }

            followerModel.Status = actor["State"]?.ToObject<int>() switch
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
                followerModel.Health = stats["Health"]?.ToObject<float>() ?? 0f;
                followerModel.Anger = stats["Anger"]?.ToObject<float>() ?? 0f;
                followerModel.Fear = stats["Fear"]?.ToObject<float>() ?? 0f;
                followerModel.Fullness = stats["Fullness"]?.ToObject<float>() ?? 0f;
                followerModel.Hydration = stats["Hydration"]?.ToObject<float>() ?? 0f;
                followerModel.Energy = stats["Energy"]?.ToObject<float>() ?? 0f;
                followerModel.Affection = stats["Affection"]?.ToObject<float>() ?? 0f;
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

            if (actor["OutfitId"]?.ToObject<int>() is { } outfitId)
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
            if (influenceMemory["UniqueId"]?.ToObject<int>() is not { } uniqueId)
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

    public bool Update(Savegame? savegame, bool createBackup)
    {
        if (savegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not JObject saveData)
        {
            return false;
        }

        var vailWorldSimToken = saveData.SelectToken("Data.VailWorldSim");
        if (vailWorldSimToken?.ToObject<string>() is not { } vailWorldSimJson ||
            JsonConverter.DeserializeRaw(vailWorldSimJson) is not JObject vailWorldSim)
        {
            return false;
        }

        var npcItemInstancesToken = saveData.SelectToken("Data.NpcItemInstances");
        var npcItemInstances = npcItemInstancesToken?.ToObject<string>() is { } npcItemInstancesJson
            ? JsonConverter.DeserializeRaw(npcItemInstancesJson)
            : null;


        var hasChanges = false;

        foreach (var actor in vailWorldSim["Actors"]?.ToList() ?? Enumerable.Empty<JToken>())
        {
            var typeId = actor["TypeId"]?.ToObject<int>();
            if (typeId is not (KelvinTypeId or VirginiaTypeId))
            {
                continue;
            }

            var followerModel = typeId == KelvinTypeId ? KelvinState : VirginiaState;

            if (actor["Position"] is { } position)
            {
                var oldPosition = position.ToObject<Position>();

                if (oldPosition != null && !oldPosition.Equals(followerModel.Pos))
                {
                    position.Replace(JToken.FromObject(followerModel.Pos));
                    hasChanges = true;
                }
            }

            var itemIdsFromFollowerModel = followerModel.Inventory.Where(item => item.Selected)
                .Select(item => item.ItemId)
                .ToHashSet();

            if (actor["EquippedItems"] is { } oldEquippedItemsToken &&
                !(oldEquippedItemsToken.ToObject<HashSet<int>>()?.SetEquals(itemIdsFromFollowerModel) ?? false))
            {
                oldEquippedItemsToken.Replace(JToken.FromObject(itemIdsFromFollowerModel));
                hasChanges = true;
            }

            if (npcItemInstances != null && actor["UniqueId"] is { } uniqueId)
            {
                if (npcItemInstances["ActorItems"] == null)
                {
                    npcItemInstances["ActorItems"] = new JArray();
                    hasChanges = true;
                }

                var actorItemsForActor = (npcItemInstances["ActorItems"]
                    ?.Children() ?? Enumerable.Empty<JToken>()).FirstOrDefault(token =>
                    token["UniqueId"]?.ToObject<int>() == uniqueId.ToObject<int>());

                if (actorItemsForActor is not { })
                {
                    actorItemsForActor = new JObject()
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

                if (actorItemsForActor.SelectToken("Items.ItemBlocks") is JArray itemBlocks)
                {
                    var actorItemsToBeRemoved = itemBlocks.Where(token =>
                            token["ItemId"]?.ToObject<int>() is { } itemId &&
                            !itemIdsFromFollowerModel.Contains(itemId))
                        .ToList();
                    actorItemsToBeRemoved.ForEach(token => token.Remove());
                    hasChanges = actorItemsToBeRemoved.Count > 0 || hasChanges;

                    var itemIdsExisting = new HashSet<int>();

                    foreach (var itemBlock in itemBlocks)
                    {
                        if (itemBlock["TotalCount"] is { } totalCountToken && totalCountToken.ToObject<int>() < 1)
                        {
                            itemBlock["TotalCount"]?.Replace(1);
                            hasChanges = true;
                        }

                        if (itemBlock["ItemId"]?.ToObject<int>() is { } itemId)
                        {
                            itemIdsExisting.Add(itemId);
                        }
                    }

                    foreach (var itemId in itemIdsFromFollowerModel.Where(itemId => !itemIdsExisting.Contains(itemId)))
                    {
                        itemBlocks.Add(JToken.FromObject(new ActorItemBlock(itemId, 1, new List<JToken>())));
                        hasChanges = true;
                    }
                }
            }

            var outfitIdToken = actor["OutfitId"];
            var oldOutfitId = outfitIdToken?.ToObject<int>() ?? 0;
            var newOutfitId = followerModel.Outfit?.Id ?? 0;

            if (oldOutfitId != newOutfitId)
            {
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

                hasChanges = true;
            }

            if (actor["Stats"] is not { } stats)
            {
                continue;
            }

            hasChanges = ModifyStat(stats, "Health", followerModel.Health) || hasChanges;
            hasChanges = ModifyStat(stats, "Anger", followerModel.Anger) || hasChanges;
            hasChanges = ModifyStat(stats, "Fear", followerModel.Fear) || hasChanges;
            hasChanges = ModifyStat(stats, "Fullness", followerModel.Fullness) || hasChanges;
            hasChanges = ModifyStat(stats, "Hydration", followerModel.Hydration) || hasChanges;
            hasChanges = ModifyStat(stats, "Energy", followerModel.Energy) || hasChanges;
            hasChanges = ModifyStat(stats, "Affection", followerModel.Affection) || hasChanges;
        }

        if (npcItemInstances != null && npcItemInstancesToken != null)
        {
            var newNpcItemInstancesJson = JsonConverter.Serialize(npcItemInstances);
            npcItemInstancesToken.Replace(newNpcItemInstancesJson);
        }

        foreach (var influenceMemory in vailWorldSim["InfluenceMemory"] ?? Enumerable.Empty<JToken>())
        {
            if (influenceMemory["UniqueId"]?.ToObject<int>() is not { } uniqueId)
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

            foreach (var influenceToken in influenceMemory["Influences"] ?? Enumerable.Empty<JToken>())
            {
                if (influenceToken["TypeId"]?.ToObject<string>() is not { } typeId)
                {
                    continue;
                }

                var newInfluence =
                    followerState.Influences.FirstOrDefault(newInfluence => newInfluence.TypeId == typeId);
                if (newInfluence == null)
                {
                    continue;
                }

                hasChanges = ModifyStat(influenceToken, "Sentiment", newInfluence.Sentiment) || hasChanges;
                hasChanges = ModifyStat(influenceToken, "Anger", newInfluence.Anger) || hasChanges;
                hasChanges = ModifyStat(influenceToken, "Fear", newInfluence.Fear) || hasChanges;
            }
        }

        if (!hasChanges)
        {
            return false;
        }

        vailWorldSimToken.Replace(JsonConverter.Serialize(vailWorldSim));

        savegame.SavegameStore.StoreJson(SavegameStore.FileType.SaveData, saveData, createBackup);
        return true;
    }

    private static bool ModifyStat(JToken stats, string key, float newValue)
    {
        if (stats[key] is not { } oldValueToken || Math.Abs(oldValueToken.ToObject<float>() - newValue) < 0.001)
        {
            return false;
        }

        oldValueToken.Replace(newValue);
        return true;
    }
}