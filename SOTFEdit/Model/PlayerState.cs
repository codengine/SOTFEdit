using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;

namespace SOTFEdit.Model;

public partial class PlayerState : ObservableObject
{
    [ObservableProperty]
    private float _currentHealth;

    [ObservableProperty]
    private float _fullness;

    [ObservableProperty]
    private float _fullnessBuff;

    [ObservableProperty]
    private float _hydration;

    [ObservableProperty]
    private float _hydrationBuff;

    [ObservableProperty]
    private float _maxHealth;

    [ObservableProperty]
    private Position _pos = new(0, 0, 0);

    [ObservableProperty]
    private float _rest;

    [ObservableProperty]
    private float _restBuff;

    [ObservableProperty]
    private Item? _selectedCloth;

    [ObservableProperty]
    private float _sickness;

    [ObservableProperty]
    private float _stamina;

    [ObservableProperty]
    private int _strengthLevel;

    partial void OnMaxHealthChanged(float value)
    {
        if (CurrentHealth > value)
        {
            CurrentHealth = value;
        }
    }

    partial void OnPosChanged(Position value)
    {
        var evt = new PlayerPosChangedEvent(value);
        WeakReferenceMessenger.Default.Send(evt);
        PoiMessenger.Instance.Send(evt);
    }

    public void Reset()
    {
        Pos = new Position(0, 0, 0);
        StrengthLevel = 0;
        CurrentHealth = 0;
        MaxHealth = 0;
        Fullness = 0;
        FullnessBuff = 0;
        Hydration = 0;
        HydrationBuff = 0;
        RestBuff = 0;
        Rest = 0;
        Stamina = 0;
        Sickness = 0;
        SelectedCloth = null;
    }
}