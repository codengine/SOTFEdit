using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model.Actors;

public partial class EquippableItem : ObservableObject
{
    [ObservableProperty]
    private bool _selected;

    public EquippableItem(Item item, bool isTemporary = false)
    {
        IsTemporary = isTemporary;
        ItemId = item.Id;
        Name = item.Name;
    }

    public bool IsTemporary { get; }
    public int ItemId { get; }
    public string Name { get; }

    private bool Equals(EquippableItem other)
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