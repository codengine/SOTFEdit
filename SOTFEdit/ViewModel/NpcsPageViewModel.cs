using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Actor;
using SOTFEdit.Model.Savegame;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public partial class NpcsPageViewModel : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly WpfObservableRangeCollection<ActorGrouping> _actorsByFamily = new();
    private readonly WpfObservableRangeCollection<ActorCollection> _actorsByType = new();
    private readonly Dictionary<int, ActorType> _actorTypes;

    [ObservableProperty] private ActorView? _actorView;

    [ObservableProperty] private ActorCollection? _selectedActorCollection;

    public NpcsPageViewModel(GameData gameData)
    {
        ActorsByFamily = new ListCollectionView(_actorsByFamily);
        ActorsByType = new ListCollectionView(_actorsByType);

        _actorTypes = gameData.ActorTypes.ToDictionary(actorType => actorType.Id);
        SetupListeners();
    }

    public ICollectionView ActorsByFamily { get; }
    public ICollectionView ActorsByType { get; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
        WeakReferenceMessenger.Default.Register<JsonModelChangedEvent>(this,
            (_, m) => { OnJsonModelChangedEvent(m); });
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
    }

    private void Reload(Savegame? selectedSavegame)
    {
        ActorView = null;
        SelectedActorCollection = null;
        if (selectedSavegame != null)
        {
            LoadNpcs(selectedSavegame);
        }
    }

    partial void OnSelectedActorCollectionChanged(ActorCollection? value)
    {
        if (value == null || SavegameManager.SelectedSavegame is not { } selectedSavegame)
        {
            ActorView = null;
        }
        else
        {
            ActorView = new ActorView(value, selectedSavegame);
        }
    }

    private void LoadNpcs(Savegame? savegame)
    {
        if (savegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType.SaveData) is not { } saveDataWrapper)
        {
            return;
        }

        if (saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.VailWorldSim) is not { } vailWorldSim)
        {
            return;
        }

        var spawnersBySpawnerUniqueId = vailWorldSim["Spawners"]?.ToObject<List<Spawner>>()?
            .ToDictionary(s => s.UniqueId) ?? new Dictionary<int, Spawner>();
        var influenceMemoryByUniqueId = vailWorldSim["InfluenceMemory"]?.ToObject<List<InfluenceMemory>>()?
            .GroupBy(s => s.UniqueId)
            .ToDictionary(s => s.Key, s => s.ToList()) ?? new Dictionary<int, List<InfluenceMemory>>();
        var killStatsByType = vailWorldSim["KillStatsList"]?.ToObject<List<KillStat>>()?
            .ToDictionary(s => s.TypeId) ?? new Dictionary<int, KillStat>();

        var actors = vailWorldSim["Actors"]?.ToObject<List<Actor>>() ?? Enumerable.Empty<Actor>().ToList();
        foreach (var actor in actors)
        {
            if (actor.GraphMask is { } graphMask)
            {
                actor.Position.AreaMask = new AreaMask(graphMask);
            }

            if (actor.SpawnerId is { } spawnerId)
            {
                actor.Spawner = spawnersBySpawnerUniqueId.GetValueOrDefault(spawnerId);
            }

            if (influenceMemoryByUniqueId.GetValueOrDefault(actor.UniqueId) is { } influenceMemory)
            {
                actor.Influences = influenceMemory.SelectMany(memory => memory.Influences).ToList();
            }

            actor.KillStats = killStatsByType.GetValueOrDefault(actor.TypeId);

            if (_actorTypes.TryGetValue(actor.TypeId, out var actorType))
            {
                actor.ActorType = actorType;
            }
            else
            {
                Logger.Warn($"No actorType definition found for {actor.TypeId}");
            }
        }

        _actorsByType.ReplaceRange(actors.OrderBy(actor => actor.GraphMask).ThenBy(actor => actor.FamilyId)
            .GroupBy(actor => actor.TypeId)
            .Select(g =>
                new ActorCollection(
                    _actorTypes.GetValueOrDefault(g.Key)?.Name ?? TranslationManager.Get("generic.unknown"),
                    g.ToList()))
            .OrderBy(c => c.Name));
        _actorsByFamily.ReplaceRange(actors.Where(actor => actor.FamilyId != null)
            .OrderBy(actor => actor.GraphMask).ThenBy(actor => actor.TypeId)
            .GroupBy(actor => actor.FamilyId)
            .GroupBy(g => BuildNameForActorList(g.ToList()))
            .Select(g =>
            {
                var grouping = new ActorGrouping(g.Key);
                grouping.ActorCollections.AddRange(g
                    .Select(familyGroup => new ActorCollection(familyGroup.Key.ToString() ?? "", familyGroup.ToList()))
                    .OrderByDescending(actorCollection => actorCollection.Actors.Count)
                    .ToList());
                return grouping;
            }).OrderBy(g => g.Name));
    }

    [RelayCommand]
    private void SelectedItemChanged(object? item)
    {
        SelectedActorCollection = item switch
        {
            ActorCollection actorCollection => actorCollection,
            null => null,
            _ => SelectedActorCollection
        };
    }

    private static string BuildNameForActorList(List<Actor> subActors)
    {
        var uniqueNames = GetUniqueNamesOfActors(subActors);
        if (uniqueNames.Count == 1)
        {
            return uniqueNames.First();
        }

        if (subActors.TrueForAll(actor => actor.ActorType?.Classification == "animal"))
        {
            return TranslationManager.Get("actors.grouping.animals");
        }

        if (subActors.TrueForAll(actor => actor.ActorType?.Classification == "creep"))
        {
            return TranslationManager.Get("actors.grouping.creeps");
        }

        if (subActors.TrueForAll(actor => actor.ActorType?.Classification == "misc"))
        {
            return TranslationManager.Get("actors.grouping.misc");
        }

        var allMale = subActors.TrueForAll(actor => actor.ActorType?.Gender == "male");
        var allFemale = subActors.TrueForAll(actor => actor.ActorType?.Gender == "female");

        if (subActors.TrueForAll(actor => actor.ActorType?.Classification == "cannibal"))
        {
            if (allMale)
            {
                return TranslationManager.Get("actors.grouping.maleCannibals");
            }

            if (allFemale)
            {
                return TranslationManager.Get("actors.grouping.femaleCannibals");
            }

            return TranslationManager.Get("actors.grouping.mixedCannibals");
        }

        if (subActors.TrueForAll(actor => actor.ActorType?.Classification == "muddy_cannibal"))
        {
            if (allMale)
            {
                return TranslationManager.Get("actors.grouping.maleMuddyCannibals");
            }

            if (allFemale)
            {
                return TranslationManager.Get("actors.grouping.femaleMuddyCannibals");
            }

            return TranslationManager.Get("actors.grouping.mixedMuddyCannibals");
        }

        if (subActors.TrueForAll(actor => actor.FamilyId == 0))
        {
            return TranslationManager.Get("actors.grouping.noFamily");
        }

        return TranslationManager.Get("actors.grouping.mixed");
    }

    private static HashSet<string> GetUniqueNamesOfActors(IEnumerable<Actor> subActors)
    {
        var uniqueNames = subActors.Where(actor => actor.ActorType?.Name != null)
            .Select(actor => actor.ActorType!.Name)
            .ToHashSet();
        return uniqueNames;
    }
}