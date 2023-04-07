using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Actor;

namespace SOTFEdit.ViewModel;

public partial class EditActorViewModel : ObservableObject
{
    [ObservableProperty] private short? _actorSelection;

    [ObservableProperty] private string _modificationMode = "Modify";

    [ObservableProperty] private bool _onlyInSameAreaAsActor = true;

    [ObservableProperty] private bool _skipKelvin = true;

    [ObservableProperty] private bool _skipVirginia = true;

    public EditActorViewModel(Actor actor, List<ActorType> allActorTypes)
    {
        Actor = actor;
        AllActorTypes = allActorTypes;
        ActorSelection = AllActorSelections.FirstOrDefault()?.Value;
        ModifyOptions.ReplaceType = Actor.ActorType;
        ModifyOptions.ActorEnergy = Actor.Stats?.GetValueOrDefault("Energy", 100f) ?? 100f;
        ModifyOptions.UpdateEnergy = !Actor.Stats?.ContainsKey("Energy") ?? true;
        ModifyOptions.ActorHealth = Actor.Stats?.GetValueOrDefault("Health", 100f) ?? 100f;
        ModifyOptions.UpdateHealth = !Actor.Stats?.ContainsKey("Energy") ?? true;

        switch (actor.TypeId)
        {
            case Constants.Actors.KelvinTypeId:
                SkipKelvin = false;
                break;
            case Constants.Actors.VirginiaTypeId:
                SkipVirginia = false;
                break;
        }

        FillInfluences();
    }

    public ModifyOptions ModifyOptions { get; } = new();

    public IEnumerable<ComboBoxItemAndValue<short>> AllActorSelections { get; } = new List<ComboBoxItemAndValue<short>>
    {
        new("This Actor", 0),
        new("All Actors of the same family", 1),
        new("All Actors of the same type", 2),
        new("All Actors", 3)
    };

    public Actor Actor { get; }
    public List<ActorType> AllActorTypes { get; }

    [RelayCommand]
    private void Save()
    {
        WeakReferenceMessenger.Default.Send(new RequestUpdateActorsEvent(this));
    }

    private void FillInfluences()
    {
        Actor.Influences ??= new List<Influence>();

        var influenceTypes = Actor.Influences.Select(influence => influence.TypeId)
            .ToHashSet();

        foreach (var typeId in Influence.AllTypes)
            if (!influenceTypes.Contains(typeId))
            {
                Actor.Influences.Add(Influence.AsFillerWithDefaults(typeId));
            }
    }
}

public partial class ModifyOptions : ObservableObject
{
    [ObservableProperty] private float _actorEnergy;
    [ObservableProperty] private float _actorHealth;
    private float? _originalEnergy;
    private float? _originalHealth;
    [ObservableProperty] private bool _removeSpawner;
    [ObservableProperty] private ActorType? _replaceType;

    [ObservableProperty] private string _teleportMode = "";
    [ObservableProperty] private bool _updateEnergy;
    [ObservableProperty] private bool _updateHealth;

    [ObservableProperty] private bool _updateInfluences;


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