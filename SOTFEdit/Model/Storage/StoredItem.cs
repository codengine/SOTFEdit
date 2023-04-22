using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.Model.SaveData.Storage.Module;
using SOTFEdit.View.Storage;

namespace SOTFEdit.Model.Storage;

public partial class StoredItem : ObservableObject
{
    private readonly int? _maxPerSlot;

    [ObservableProperty]
    private int _count;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Max))]
    [NotifyCanExecuteChangedFor(nameof(SetToMaxCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
    private ItemWrapper? _selectedItem;

    public StoredItem(ItemWrapper? item, int count, List<ItemWrapper> supportedItems, int? maxPerSlot,
        List<IStorageModule>? modules = null)
    {
        _maxPerSlot = maxPerSlot;
        SelectedItem = item;
        Count = count;
        SupportedItems = supportedItems;
        Modules = modules;
    }

    public List<ItemWrapper> SupportedItems { get; }
    public List<IStorageModule>? Modules { get; }

    public int Max => _maxPerSlot ?? SelectedItem?.Max ?? 0;

    partial void OnSelectedItemChanged(ItemWrapper? value)
    {
        Count = value == null ? 0 : Max;
    }

    [RelayCommand(CanExecute = nameof(HasItem))]
    public void Clear()
    {
        SelectedItem = null;
    }

    [RelayCommand(CanExecute = nameof(HasItem))]
    private void SetToMax()
    {
        if (HasItem())
        {
            Count = Max;
        }
    }

    public bool HasItem()
    {
        return SelectedItem != null;
    }
}