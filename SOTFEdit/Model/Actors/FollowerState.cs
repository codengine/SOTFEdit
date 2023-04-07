using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SOTFEdit.Model.SaveData.Actor;

namespace SOTFEdit.Model.Actors;

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

    [ObservableProperty] private Position _pos = new(0, 0, 0);

    [ObservableProperty] private string _status = "???";

    [ObservableProperty] private int? _uniqueId;


    public FollowerState(int typeId, List<Outfit> outfits, IEnumerable<Item> equippableItems)
    {
        TypeId = typeId;
        Outfits = outfits;
        foreach (var equippableItem in equippableItems.Select(item => new EquippableItem(item)))
            Inventory.Add(equippableItem);
    }

    public int TypeId { get; }

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
        foreach (var equippableItem in Inventory) equippableItem.Selected = false;

        var temporaryItems = Inventory.Where(item => item.IsTemporary).ToList();
        temporaryItems.ForEach(temporaryItem => Inventory.Remove(temporaryItem));

        Outfit = null;
        UniqueId = null;
        Influences.Clear();
    }

    public HashSet<int> GetSelectedInventoryItemIds()
    {
        return Inventory.Where(item => item.Selected)
            .Select(item => item.ItemId)
            .ToHashSet();
    }
}