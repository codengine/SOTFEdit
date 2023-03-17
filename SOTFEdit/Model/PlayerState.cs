using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model;

public partial class PlayerState : ObservableObject
{
    [NotifyPropertyChangedFor(nameof(PositionPrintable))] [ObservableProperty]
    private Position _position = new(0, 0, 0);

    public string PositionPrintable => $"X: {Position.X}, Y: {Position.Y}, Z: {Position.Z}";

    [ObservableProperty] private int _strengthLevel;
    [ObservableProperty] private float _maxHealth;
    [ObservableProperty] private float _currentHealth;
    [ObservableProperty] private float _fullness;
    [ObservableProperty] private float _hydration;
    [ObservableProperty] private float _rest;
    [ObservableProperty] private float _stamina;

    partial void OnMaxHealthChanged(float value)
    {
        if (CurrentHealth > value)
        {
            CurrentHealth = value;
        }
    }

    public void Reset()
    {
        Position = new Position(0, 0, 0);
        StrengthLevel = 0;
        CurrentHealth = 0;
        MaxHealth = 0;
        Fullness = 0;
        Hydration = 0;
        Rest = 0;
        Stamina = 0;
    }
}