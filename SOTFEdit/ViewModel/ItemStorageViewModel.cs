using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.Model.Storage;
using SOTFEdit.View.Storage;

namespace SOTFEdit.ViewModel;

public partial class ItemStorageViewModel : ObservableObject
{
    [ObservableProperty] private int _count;

    [NotifyPropertyChangedFor(nameof(Max))] [NotifyCanExecuteChangedFor(nameof(FillAllCommand))] [ObservableProperty]
    private ItemWrapper? _selectedItemForAll;

    public ItemStorageViewModel(BaseStorage itemsStorage)
    {
        ItemsStorage = itemsStorage;
    }

    public BaseStorage ItemsStorage { get; }

    public int Max => SelectedItemForAll?.Max ?? 1000;

    partial void OnSelectedItemForAllChanged(ItemWrapper? value)
    {
        Count = value?.Max ?? 1;
    }

    [RelayCommand]
    public void ClearAll()
    {
        foreach (var slot in ItemsStorage.Slots)
        {
            foreach (var storedItem in slot.StoredItems)
            {
                storedItem.Clear();
            }
        }
    }

    [RelayCommand(CanExecute = nameof(HasItemSelected))]
    public void FillAll()
    {
        if (SelectedItemForAll is not { } selectedItem)
        {
            return;
        }

        foreach (var slot in ItemsStorage.Slots)
        {
            foreach (var storedItem in slot.StoredItems)
            {
                storedItem.SelectedItem = selectedItem;
                storedItem.Count = Count;
            }
        }
    }

    [RelayCommand]
    public void SetAllToMax()
    {
        foreach (var slot in ItemsStorage.Slots)
        {
            foreach (var storedItem in slot.StoredItems)
            {
                if (storedItem.SelectedItem != null)
                {
                    storedItem.Count = storedItem.Max;
                }
            }
        }
    }

    public bool HasItemSelected()
    {
        return SelectedItemForAll != null;
    }
}