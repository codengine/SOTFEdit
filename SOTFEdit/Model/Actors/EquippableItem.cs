using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model.Actors;

public partial class EquippableItem(Item item, bool isTemporary = false) : ObservableObject
{
    [ObservableProperty] private bool _selected;

    public bool IsTemporary { get; } = isTemporary;
    public int ItemId { get; } = item.Id;
    public string Name { get; } = item.Name;

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