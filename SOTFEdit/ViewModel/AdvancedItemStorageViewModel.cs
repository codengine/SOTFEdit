using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Storage;
using SOTFEdit.View.Storage;

namespace SOTFEdit.ViewModel;

public partial class AdvancedItemStorageViewModel : ObservableObject
{
    [ObservableProperty]
    private int _count;

    [NotifyPropertyChangedFor(nameof(Max))]
    [NotifyCanExecuteChangedFor(nameof(FillAllCommand))]
    [ObservableProperty]
    private ItemWrapper? _selectedItemForAll;

    public AdvancedItemStorageViewModel(AdvancedItemsStorage itemsStorage)
    {
        ItemsStorage = itemsStorage;
    }

    public AdvancedItemsStorage ItemsStorage { get; }

    public int Max => SelectedItemForAll?.Max ?? 1000;

    partial void OnSelectedItemForAllChanged(ItemWrapper? value)
    {
        Count = value?.Max ?? 1;
    }

    [RelayCommand]
    private void ApplyToAllOfSameType()
    {
        WeakReferenceMessenger.Default.Send(new ApplyToAllOfSameTypeEvent(ItemsStorage));
    }

    [RelayCommand]
    private void ClearAll()
    {
        foreach (var slot in ItemsStorage.Slots)
        foreach (var storedItem in slot.StoredItems)
        {
            storedItem.Clear();
        }
    }

    [RelayCommand(CanExecute = nameof(HasItemSelected))]
    private void FillAll()
    {
        if (SelectedItemForAll is not { } selectedItem)
        {
            return;
        }

        foreach (var slot in ItemsStorage.Slots)
        foreach (var storedItem in slot.StoredItems)
        {
            var candidate = storedItem.SupportedItems.FirstOrDefault(item => item.Item.Id == selectedItem.Item.Id);
            if (candidate == null)
            {
                continue;
            }

            storedItem.SelectedItem = candidate;
            storedItem.Count = Count;
        }
    }

    [RelayCommand]
    private void SetAllToMax()
    {
        foreach (var slot in ItemsStorage.Slots)
        foreach (var storedItem in slot.StoredItems)
        {
            if (storedItem.SelectedItem != null)
            {
                storedItem.Count = storedItem.Max;
            }
        }
    }

    public bool HasItemSelected()
    {
        return SelectedItemForAll != null;
    }
}