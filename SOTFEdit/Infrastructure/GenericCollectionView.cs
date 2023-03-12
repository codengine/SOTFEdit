using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace SOTFEdit.Infrastructure;

//Source: https://stackoverflow.com/questions/20254690/wpf-icollectionview-binding-item-cannot-resolve-property-of-type-object
public class GenericCollectionView<T> : ICollectionView<T>, IEditableCollectionView<T>
{
    private readonly ListCollectionView _collectionView;

    public GenericCollectionView(ListCollectionView collectionView)
    {
        _collectionView = collectionView ?? throw new ArgumentNullException(nameof(collectionView));
    }

    public CultureInfo Culture
    {
        get => _collectionView.Culture;
        set => _collectionView.Culture = value;
    }

    public IEnumerable SourceCollection => _collectionView.SourceCollection;

    public Predicate<object> Filter
    {
        get => _collectionView.Filter;
        set => _collectionView.Filter = value;
    }

    public bool CanFilter => _collectionView.CanFilter;

    public SortDescriptionCollection SortDescriptions => _collectionView.SortDescriptions;

    public bool CanSort => _collectionView.CanSort;

    public bool CanGroup => _collectionView.CanGroup;

    public ObservableCollection<GroupDescription> GroupDescriptions => _collectionView.GroupDescriptions;

    public ReadOnlyObservableCollection<object> Groups => _collectionView.Groups;

    public bool IsEmpty => _collectionView.IsEmpty;

    public object CurrentItem => _collectionView.CurrentItem;

    public int CurrentPosition => _collectionView.CurrentPosition;

    public bool IsCurrentAfterLast => _collectionView.IsCurrentAfterLast;

    public bool IsCurrentBeforeFirst => _collectionView.IsCurrentBeforeFirst;

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => ((ICollectionView)_collectionView).CollectionChanged += value;
        remove => ((ICollectionView)_collectionView).CollectionChanged -= value;
    }

    public event CurrentChangingEventHandler CurrentChanging
    {
        add => ((ICollectionView)_collectionView).CurrentChanging += value;
        remove => ((ICollectionView)_collectionView).CurrentChanging -= value;
    }

    public event EventHandler CurrentChanged
    {
        add => ((ICollectionView)_collectionView).CurrentChanged += value;
        remove => ((ICollectionView)_collectionView).CurrentChanged -= value;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return (IEnumerator<T>)((ICollectionView)_collectionView).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((ICollectionView)_collectionView).GetEnumerator();
    }

    public bool Contains(object item)
    {
        return _collectionView.Contains(item);
    }

    public void Refresh()
    {
        _collectionView.Refresh();
    }

    public IDisposable DeferRefresh()
    {
        return _collectionView.DeferRefresh();
    }

    public bool MoveCurrentToFirst()
    {
        return _collectionView.MoveCurrentToFirst();
    }

    public bool MoveCurrentToLast()
    {
        return _collectionView.MoveCurrentToLast();
    }

    public bool MoveCurrentToNext()
    {
        return _collectionView.MoveCurrentToNext();
    }

    public bool MoveCurrentToPrevious()
    {
        return _collectionView.MoveCurrentToPrevious();
    }

    public bool MoveCurrentTo(object item)
    {
        return _collectionView.MoveCurrentTo(item);
    }

    public bool MoveCurrentToPosition(int position)
    {
        return _collectionView.MoveCurrentToPosition(position);
    }

    public NewItemPlaceholderPosition NewItemPlaceholderPosition
    {
        get => _collectionView.NewItemPlaceholderPosition;
        set => _collectionView.NewItemPlaceholderPosition = value;
    }

    public bool CanAddNew => _collectionView.CanAddNew;

    public bool IsAddingNew => _collectionView.IsAddingNew;

    public object CurrentAddItem => _collectionView.CurrentAddItem;

    public bool CanRemove => _collectionView.CanRemove;

    public bool CanCancelEdit => _collectionView.CanCancelEdit;

    public bool IsEditingItem => _collectionView.IsEditingItem;

    public object CurrentEditItem => _collectionView.CurrentEditItem;

    public object AddNew()
    {
        return _collectionView.AddNew();
    }

    public void CommitNew()
    {
        _collectionView.CommitNew();
    }

    public void CancelNew()
    {
        _collectionView.CancelNew();
    }

    public void RemoveAt(int index)
    {
        _collectionView.RemoveAt(index);
    }

    public void Remove(object item)
    {
        _collectionView.Remove(item);
    }

    public void EditItem(object item)
    {
        _collectionView.EditItem(item);
    }

    public void CommitEdit()
    {
        _collectionView.CommitEdit();
    }

    public void CancelEdit()
    {
        _collectionView.CancelEdit();
    }
}

public interface ICollectionView<out T> : IEnumerable<T>, ICollectionView
{
}

public interface IEditableCollectionView<T> : IEditableCollectionView
{
}