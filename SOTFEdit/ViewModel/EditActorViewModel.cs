using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Actor;

namespace SOTFEdit.ViewModel;

public partial class EditActorViewModel : ObservableObject
{
    [ObservableProperty]
    private short? _actorSelection;

    [ObservableProperty]
    private ActorModificationMode _modificationMode = ActorModificationMode.Modify;

    [ObservableProperty]
    private bool _onlyInSameAreaAsActor = true;

    [ObservableProperty]
    private bool _skipKelvin = true;

    [ObservableProperty]
    private bool _skipVirginia = true;

    public EditActorViewModel(Actor actor, List<ActorType> allActorTypes)
    {
        Actor = actor;
        AllActorTypes = allActorTypes;
        ActorSelection = AllActorSelections.FirstOrDefault()?.Value;
        ModifyOptions.ReplaceType = allActorTypes.First();
        ModifyOptions.ActorEnergy = Actor.Stats?.GetValueOrDefault("Energy", 100f) ?? 100f;
        ModifyOptions.UpdateEnergy = !Actor.Stats?.ContainsKey("Energy") ?? true;
        ModifyOptions.ActorHealth = Actor.Stats?.GetValueOrDefault("Health", 100f) ?? 100f;
        ModifyOptions.UpdateHealth = !Actor.Stats?.ContainsKey("Energy") ?? true;

        Influences = actor.Influences is { } influences
            ? new ObservableCollectionEx<Influence>(influences)
            : new ObservableCollectionEx<Influence>();
        Influences.CollectionChanged += InfluencesOnCollectionChanged;

        AllInfluences = Influence.AllTypes.Select(type =>
                new ComboBoxItemAndValue<string>(TranslationManager.Get("actors.influenceType." + type), type))
            .ToList();

        switch (actor.TypeId)
        {
            case Constants.Actors.KelvinTypeId:
                SkipKelvin = false;
                break;
            case Constants.Actors.VirginiaTypeId:
                SkipVirginia = false;
                break;
        }
    }

    public ObservableCollectionEx<Influence> Influences { get; }

    public List<ComboBoxItemAndValue<string>> AllInfluences { get; }

    public ModifyOptions ModifyOptions { get; } = new();

    public IEnumerable<ComboBoxItemAndValue<short>> AllActorSelections { get; } = new List<ComboBoxItemAndValue<short>>
    {
        new(TranslationManager.Get("actors.modificationOptions.allActorSelections.thisActor"), 0),
        new(TranslationManager.Get("actors.modificationOptions.allActorSelections.allActorsOfSameFamily"), 1),
        new(TranslationManager.Get("actors.modificationOptions.allActorSelections.allActorsOfSameType"), 2),
        new(TranslationManager.Get("actors.modificationOptions.allActorSelections.allActors"), 3)
    };

    public Actor Actor { get; }

    public List<ActorType> AllActorTypes { get; }

    private void InfluencesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ModifyOptions.UpdateInfluences = true;
    }

    [RelayCommand]
    private void Save()
    {
        WeakReferenceMessenger.Default.Send(new RequestUpdateActorsEvent(this));
    }
}

public partial class ModifyOptions : ObservableObject
{
    [ObservableProperty]
    private float _actorEnergy;

    [ObservableProperty]
    private float _actorHealth;

    private float? _originalEnergy;
    private float? _originalHealth;

    [ObservableProperty]
    private bool _removeSpawner;

    [ObservableProperty]
    private ActorType? _replaceType;

    [ObservableProperty]
    private string _teleportMode = "";

    [ObservableProperty]
    private bool _updateEnergy;

    [ObservableProperty]
    private bool _updateHealth;

    [ObservableProperty]
    private bool _updateInfluences;


    partial void OnActorEnergyChanging(float value)
    {
        _originalEnergy ??= value;

        if (_originalEnergy != null && Math.Abs(_originalEnergy.Value - value) < 0.01)
        {
            UpdateEnergy = false;
        }
        else if (Math.Abs(ActorEnergy - value) > 0.01)
        {
            UpdateEnergy = true;
        }
    }

    partial void OnActorHealthChanging(float value)
    {
        _originalHealth ??= value;

        if (_originalHealth != null && Math.Abs(_originalHealth.Value - value) < 0.01)
        {
            UpdateHealth = false;
        }
        else if (Math.Abs(ActorHealth - value) > 0.01)
        {
            UpdateHealth = true;
        }
    }
}