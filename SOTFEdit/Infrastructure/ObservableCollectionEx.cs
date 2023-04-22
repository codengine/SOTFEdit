using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SOTFEdit.Infrastructure;

public class ObservableCollectionEx<T> : ObservableCollection<T>
{
    private bool _suppressNotification;

    public ObservableCollectionEx(IEnumerable<T> collection) : base(collection)
    {
    }

    public ObservableCollectionEx()
    {
    }

    public new void SetItem(int index, T item)
    {
        base.SetItem(index, item);
    }

    public void RemoveRange(IEnumerable<T> itemsToRemove)
    {
        if (itemsToRemove == null)
        {
            throw new ArgumentNullException(nameof(itemsToRemove));
        }

        _suppressNotification = true;

        var removedItems = new List<T>();
        var removedIndices = new List<int>();
        foreach (var item in itemsToRemove)
        {
            var index = IndexOf(item);
            if (index < 0)
            {
                continue;
            }

            removedItems.Add(item);
            removedIndices.Add(index);
            RemoveItem(index);
        }

        _suppressNotification = false;
        for (var i = 0; i < removedItems.Count; i++)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                removedItems[i], removedIndices[i]));
        }
    }

    public void AddRange(IEnumerable<T> itemsToAdd)
    {
        if (itemsToAdd == null)
        {
            throw new ArgumentNullException(nameof(itemsToAdd));
        }

        foreach (var item in itemsToAdd)
        {
            Add(item);
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppressNotification)
        {
            base.OnCollectionChanged(e);
        }
    }

    public void RemoveAndAdd(Predicate<T> check, List<T> newItems)
    {
        for (var i = Count - 1; i >= 0; i--)
        {
            if (check.Invoke(this[i]))
            {
                RemoveAt(i);
            }
        }

        foreach (var item in newItems)
        {
            Add(item);
        }
    }

    public void ReplaceRange(IEnumerable<T> elementsToAdd)
    {
        _suppressNotification = true;
        Clear();
        AddRange(elementsToAdd);
        _suppressNotification = false;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}