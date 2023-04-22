using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Infrastructure.Converters;
using SOTFEdit.Model.Events;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Map;

public abstract partial class BasePoi : ObservableObject, IPoi
{
    [NotifyPropertyChangedFor(nameof(Visible))]
    [ObservableProperty]
    private bool _enabled;

    [NotifyPropertyChangedFor(nameof(Visible))]
    [ObservableProperty]
    private bool _filtered;

    [NotifyPropertyChangedFor(nameof(IconZIndex))]
    [ObservableProperty]
    private bool _isSelected;

    private float _left;

    [NotifyCanExecuteChangedFor(nameof(TeleportPlayerHereCommand))]
    [NotifyCanExecuteChangedFor(nameof(TeleportKelvinHereCommand))]
    [NotifyCanExecuteChangedFor(nameof(TeleportVirginiaHereCommand))]
    [ObservableProperty]
    private Position? _position;

    private float _top;

    [ObservableProperty]
    private float _x;

    [ObservableProperty]
    private float _z;

    protected BasePoi(Position position)
    {
        _position = position;
        _x = position.X;
        _z = position.Z;
        (_left, _top) = CoordinateConverter.IngameToPixel(position.X, position.Z);
    }

    protected BasePoi(float x, float y, Position? position)
    {
        _x = x;
        _z = y;
        (_left, _top) = CoordinateConverter.IngameToPixel(x, y);
        _position = position;
    }

    protected static BitmapImage DefaultIcon => LoadBaseIcon("question-mark.png");
    protected virtual int IconOffset => 16;

    public string? AreaName => Position?.Area.Name;

    public string PrintableCoordinates =>
        Position != null ? $"X: {Position.X:F2}, Y: {Position.Y:F2}, Z: {Position.Z:F2}" : $"X: {X}, Z: {Z}";

    public virtual bool IsUnderground => Position?.Area.IsUnderground() ?? false;

    public bool Visible => _enabled && !Filtered;

    public abstract BitmapImage Icon { get; }

    public virtual int IconWidth => 32;
    public virtual int IconHeight => 32;
    public virtual int IconZIndex => IsSelected ? 99 : 0;

    public virtual void ApplyFilter(MapFilter mapFilter)
    {
        Filtered = ShouldFilter(mapFilter);
    }

    public float IconLeft => _left - IconOffset;

    public float IconTop => _top - IconOffset;

    public virtual string Title => "";
    public string? Description { get; init; }

    partial void OnPositionChanged(Position? value)
    {
        if (value != null)
        {
            X = value.X;
            Z = value.Z;
            (_left, _top) = CoordinateConverter.IngameToPixel(value.X, value.Z);
        }

        OnPropertyChanged(nameof(PrintableCoordinates));
        OnPropertyChanged(nameof(IconLeft));
        OnPropertyChanged(nameof(IconTop));
        TeleportPlayerHereCommand.NotifyCanExecuteChanged();
        TeleportKelvinHereCommand.NotifyCanExecuteChanged();
        TeleportVirginiaHereCommand.NotifyCanExecuteChanged();
        SpawnActorsCommand.NotifyCanExecuteChanged();
    }

    protected virtual bool ShouldFilter(MapFilter mapFilter)
    {
        if (mapFilter.AreaFilter != AreaFilter.All)
        {
            return !mapFilter.AreaFilter.ShouldInclude(this);
        }

        return false;
    }

    protected static BitmapImage LoadBaseIcon(string icon, int? width = null, int? height = null)
    {
        return $"/images/icons/{icon}".LoadAppLocalImage(width, height);
    }

    [RelayCommand(CanExecute = nameof(CanTeleportPlayer))]
    private void TeleportPlayerHere()
    {
        if (CanTeleportPlayer())
        {
            PoiMessenger.Instance.Send(new ShowTeleportWindowEvent(this,
                MapTeleportWindowViewModel.TeleportationMode.Player));
        }
    }

    private bool CanTeleportVirginia()
    {
        return Position != null && !IsUnderground && !IsActorOfType(Constants.Actors.VirginiaTypeId);
    }

    private bool IsActorOfType(int typeId)
    {
        return this is ActorPoi actorPoi && actorPoi.Actor.TypeId == typeId;
    }

    private bool CanTeleportKelvin()
    {
        return Position != null && !IsUnderground && !IsActorOfType(Constants.Actors.KelvinTypeId);
    }

    [RelayCommand(CanExecute = nameof(CanTeleportKelvin))]
    private void TeleportKelvinHere()
    {
        if (CanTeleportKelvin())
        {
            PoiMessenger.Instance.Send(new ShowTeleportWindowEvent(this,
                MapTeleportWindowViewModel.TeleportationMode.Kelvin));
        }
    }

    private bool CanTeleportPlayer()
    {
        return Position != null && this is not PlayerPoi;
    }

    [RelayCommand(CanExecute = nameof(CanTeleportVirginia))]
    private void TeleportVirginiaHere()
    {
        if (CanTeleportVirginia())
        {
            PoiMessenger.Instance.Send(new ShowTeleportWindowEvent(this,
                MapTeleportWindowViewModel.TeleportationMode.Virginia));
        }
    }

    private bool CanSpawnActors()
    {
        return Position != null;
    }

    [RelayCommand(CanExecute = nameof(CanSpawnActors))]
    private void SpawnActors()
    {
        PoiMessenger.Instance.Send(new ShowSpawnActorsWindowEvent(this));
    }
}

public interface IPoi
{
    // ReSharper disable once UnusedMemberInSuper.Global
    public Position? Position { get; }
    public string PrintableCoordinates { get; }
    public bool IsUnderground { get; }
    public string? AreaName { get; }
    public BitmapImage Icon { get; }
    public float IconLeft { get; }
    public float IconTop { get; }
    public string Title { get; }
    public string? Description { get; }
    public bool Enabled { get; set; }
    public bool Visible { get; }
    public int IconWidth { get; }
    public int IconHeight { get; }
    public int IconZIndex { get; }
    public bool IsSelected { get; set; }
    public void ApplyFilter(MapFilter mapFilter);
}