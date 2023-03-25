﻿using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.View.Storage;

namespace SOTFEdit.Model.Storage;

public partial class StoredItem : ObservableObject
{
    private readonly int? _maxPerSlot;

    [ObservableProperty] private int _count;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Max))]
    [NotifyCanExecuteChangedFor(nameof(SetToMaxCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
    private ItemWrapper? _selectedItem;

    public StoredItem(ItemWrapper? item, int count, List<ItemWrapper> supportedItems, int? maxPerSlot)
    {
        _maxPerSlot = maxPerSlot;
        SelectedItem = item;
        Count = count;
        SupportedItems = supportedItems;
    }

    public List<ItemWrapper> SupportedItems { get; }

    public int Max => _maxPerSlot ?? SelectedItem?.Max ?? 0;

    partial void OnSelectedItemChanged(ItemWrapper? value)
    {
        Count = value == null ? 0 : 1;
    }

    [RelayCommand(CanExecute = nameof(HasItem))]
    public void Clear()
    {
        SelectedItem = null;
    }

    [RelayCommand(CanExecute = nameof(HasItem))]
    private void SetToMax()
    {
        if (SelectedItem is { })
        {
            Count = Max;
        }
    }

    public bool HasItem()
    {
        return SelectedItem != null;
    }
}