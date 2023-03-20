using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model;

public partial class FollowerState : ObservableObject
{
    [ObservableProperty] private float _affection;
    [ObservableProperty] private float _anger;
    [ObservableProperty] private float _energy;
    [ObservableProperty] private float _fear;
    [ObservableProperty] private float _fullness;

    [ObservableProperty] private float _health;
    [ObservableProperty] private float _hydration;
    [ObservableProperty] private Outfit? _outfit;

    [NotifyPropertyChangedFor(nameof(PositionPrintable))] [ObservableProperty]
    private Position _pos = new(0, 0, 0);

    [ObservableProperty] private string _status = "???";

    [ObservableProperty] private int? _uniqueId;


    public FollowerState(int typeId, List<Outfit> outfits, IEnumerable<Item> equippableItems)
    {
        TypeId = typeId;
        Outfits = outfits;
        foreach (var equippableItem in equippableItems.Select(item => new EquippableItem(item)))
        {
            Inventory.Add(equippableItem);
        }
    }

    public int TypeId { get; }

    public string PositionPrintable => $"X: {Pos.X}, Y: {Pos.Y}, Z: {Pos.Z}";
    public List<Outfit> Outfits { get; }
    public ObservableCollection<EquippableItem> Inventory { get; } = new();
    public ObservableCollection<Influence> Influences { get; } = new();

    public void Reset()
    {
        Status = "???";
        Pos = new Position(0, 0, 0);
        Health = 0.0f;
        Anger = 0.0f;
        Fear = 0.0f;
        Fullness = 0.0f;
        Hydration = 0.0f;
        Energy = 0.0f;
        Affection = 0.0f;
        foreach (var equippableItem in Inventory)
        {
            equippableItem.Selected = false;
        }

        var temporaryItems = Inventory.Where(item => item.IsTemporary).ToList();
        temporaryItems.ForEach(temporaryItem => Inventory.Remove(temporaryItem));

        Outfit = null;
        UniqueId = null;
        Influences.Clear();
    }
}

public partial class Influence : ObservableObject
{
    public string TypeId { get; set; }
    [ObservableProperty] private float _sentiment;
    [ObservableProperty] private float _anger;
    [ObservableProperty] private float _fear;
}

public partial class EquippableItem : ObservableObject
{
    [ObservableProperty] private bool _selected;

    public EquippableItem(Item item, bool isTemporary = false)
    {
        IsTemporary = isTemporary;
        ItemId = item.Id;
        Name = item.Name;
    }

    public bool IsTemporary { get; }
    public int ItemId { get; }
    public string Name { get; }

    protected bool Equals(EquippableItem other)
    {
        return ItemId == other.ItemId;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((EquippableItem)obj);
    }

    public override int GetHashCode()
    {
        return ItemId;
    }
}