using System;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using static SOTFEdit.Model.Constants.Actors;

namespace SOTFEdit.ViewModel;

public partial class FollowerPageViewModel : ObservableObject
{
    public FollowerState KelvinState { get; } = new(KelvinTypeId);
    public FollowerState VirginiaState { get; } = new(VirginiaTypeId);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReviveCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToKelvinCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToVirginiaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToPlayerCommand))]
    private Savegame? _selectedSavegame;

    public FollowerPageViewModel()
    {
        SetupListeners();
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

        follower.Position = Ioc.Default.GetRequiredService<PlayerPageViewModel>().PlayerState.Position.Copy();
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    public void MoveToKelvin(FollowerState follower)
    {
        follower.Position = KelvinState.Position.Copy();
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    public void MoveToVirginia(FollowerState follower)
    {
        follower.Position = VirginiaState.Position.Copy();
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
            followerModel.Status = actor["State"]?.ToObject<int>() switch
            {
                2 => "Alive",
                6 => "Dead",
                _ => "???"
            };

            if (actor["Position"] is { } position)
            {
                followerModel.Position = position.ToObject<Position>() ?? new Position(0, 0, 0);
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

        var hasChanges = false;

        foreach (var actor in vailWorldSim["Actors"] ?? Enumerable.Empty<JToken>())
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

                if (oldPosition != null && !oldPosition.Equals(followerModel.Position))
                {
                    position.Replace(JToken.FromObject(followerModel.Position));
                    hasChanges = true;
                }
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
        if (stats[key] is not { } oldValueToken || oldValueToken?.ToObject<float>() is not { } oldValue ||
            Math.Abs(oldValue - newValue) < 0.001)
        {
            return false;
        }

        oldValueToken.Replace(newValue);
        return true;
    }
}