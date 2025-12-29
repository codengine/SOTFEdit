using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public partial class PoiGroup(
    bool enabled, IEnumerable<IPoi> pois, string title, string groupKey,
    PoiGroupType groupType, BitmapImage? icon = null)
    : ObservableObject, IPoiGrouper
{
    [ObservableProperty] private bool _enabled = enabled;

    public PoiGroup(bool enabled, IEnumerable<IPoi> pois, string title, PoiGroupType groupType,
        BitmapImage? icon = null) : this(enabled, pois, title, groupType.ToString(), groupType, icon)
    {
    }

    public HashSet<IPoi> Pois { get; } = [..pois];

    public int Count => Pois.Count;
    public BitmapImage? Icon { get; } = icon;
    public string GroupKey { get; } = groupKey;
    public string BaseTitle { get; } = title;
    public string Title => $"{BaseTitle} ({Count})";
    public PoiGroupType GroupType { get; } = groupType;

    public void SetEnabledNoRefresh(bool value)
    {
        SetEnabledNoRefresh(value, true);
    }

    public void SetEnabledNoRefresh(bool value, bool emitEvents)
    {
        if (emitEvents)
        {
            foreach (var poi in Pois)
            {
                poi.Enabled = value;
            }
        }
        else
        {
            foreach (var poi in Pois)
            {
                poi.SetEnabledNoRefresh(value);
            }
        }

#pragma warning disable MVVMTK0034
        var oldValue = _enabled;
        _enabled = value;
#pragma warning restore MVVMTK0034

        if (emitEvents && oldValue != value)
        {
            OnPropertyChanged(nameof(Enabled));
        }
    }

    partial void OnEnabledChanged(bool value)
    {
        foreach (var poi in Pois)
        {
            poi.Enabled = value;
        }

        if (value)
        {
            PoiMessenger.Instance.Send(new PoiRefreshEvent(this));
        }
    }

    public bool Remove(IEnumerable<IPoi> pois)
    {
        var removed = pois.Aggregate(false, (current, poi) => Pois.Remove(poi) || current);

        if (removed)
        {
            OnPropertyChanged(nameof(Title));
        }

        return removed;
    }

    public int RemoveWhere(Predicate<IPoi> predicate)
    {
        var countRemoved = Pois.RemoveWhere(predicate);
        if (countRemoved > 0)
        {
            OnPropertyChanged(Title);
        }

        return countRemoved;
    }
}