using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model;

public partial class PlayerState : ObservableObject
{
    [ObservableProperty] private float _currentHealth;
    [ObservableProperty] private float _fullness;
    [ObservableProperty] private float _hydration;
    [ObservableProperty] private float _maxHealth;

    [NotifyPropertyChangedFor(nameof(PositionPrintable))] [ObservableProperty]
    private Position _pos = new(0, 0, 0);

    [ObservableProperty] private float _rest;
    [ObservableProperty] private float _stamina;

    [ObservableProperty] private int _strengthLevel;

    public string PositionPrintable => $"X: {Pos.X}, Y: {Pos.Y}, Z: {Pos.Z}";

    partial void OnMaxHealthChanged(float value)
    {
        if (CurrentHealth > value)
        {
            CurrentHealth = value;
        }
    }

    public void Reset()
    {
        Pos = new Position(0, 0, 0);
        StrengthLevel = 0;
        CurrentHealth = 0;
        MaxHealth = 0;
        Fullness = 0;
        Hydration = 0;
        Rest = 0;
        Stamina = 0;
    }
}