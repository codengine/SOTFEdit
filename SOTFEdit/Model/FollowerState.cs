using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model;

public partial class FollowerState : ObservableObject
{
    public int TypeId { get; }

    public FollowerState(int typeId)
    {
        TypeId = typeId;
    }

    [ObservableProperty] private string _status = "???";

    [NotifyPropertyChangedFor(nameof(PositionPrintable))] [ObservableProperty]
    private Position _position = new(0, 0, 0);

    public string PositionPrintable => $"X: {Position.X}, Y: {Position.Y}, Z: {Position.Z}";

    [ObservableProperty] private float _health;
    [ObservableProperty] private float _anger;
    [ObservableProperty] private float _fear;
    [ObservableProperty] private float _fullness;
    [ObservableProperty] private float _hydration;
    [ObservableProperty] private float _energy;
    [ObservableProperty] private float _affection;

    public void Reset()
    {
        Status = "???";
        Position = new Position(0, 0, 0);
        Health = 0.0f;
        Anger = 0.0f;
        Fear = 0.0f;
        Fullness = 0.0f;
        Hydration = 0.0f;
        Energy = 0.0f;
        Affection = 0.0f;
    }
}