using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public partial class PoiGroup : ObservableObject, IPoiGrouper
{
    [ObservableProperty]
    private bool _enabled;

    public PoiGroup(bool enabled, IEnumerable<IPoi> pois, string title, PoiGroupType groupType,
        BitmapImage? icon = null) : this(enabled, pois, title, groupType.ToString(), groupType, icon)
    {
    }

    public PoiGroup(bool enabled, IEnumerable<IPoi> pois, string title, string groupKey,
        PoiGroupType groupType, BitmapImage? icon = null)
    {
        _enabled = enabled;
        Icon = icon;
        GroupType = groupType;
        Pois = new HashSet<IPoi>(pois);
        BaseTitle = title;
        GroupKey = groupKey;
    }

    public HashSet<IPoi> Pois { get; }

    public int Count => Pois.Count;
    public BitmapImage? Icon { get; }
    public string GroupKey { get; }
    public string BaseTitle { get; }
    public string Title => $"{BaseTitle} ({Count})";
    public PoiGroupType GroupType { get; }

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

        var oldValue = _enabled;
        _enabled = value;

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