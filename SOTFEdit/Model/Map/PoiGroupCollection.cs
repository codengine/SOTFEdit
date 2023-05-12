using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Companion.Shared;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public partial class PoiGroupCollection : ObservableObject, IPoiGrouper
{
    [ObservableProperty]
    private bool _enabled;

    public PoiGroupCollection(bool enabled, string title, string groupKey, IEnumerable<PoiGroup> groups,
        PoiGroupType groupType)
    {
        _enabled = enabled;
        GroupType = groupType;
        PoiGroups = new List<PoiGroup>(groups);
        BaseTitle = title;
        GroupKey = groupKey;
    }

    public List<PoiGroup> PoiGroups { get; }

    private int Count => PoiGroups.Select(group => group.Count).Sum();
    public string GroupKey { get; }

    public void SetEnabledNoRefresh(bool value)
    {
        SetEnabledNoRefresh(value, false);
    }

    public string Title => $"{BaseTitle} ({Count})";
    public string BaseTitle { get; }
    public PoiGroupType GroupType { get; }

    public void SetEnabledNoRefresh(bool value, bool collectionOnly)
    {
        _enabled = value;
        if (collectionOnly)
        {
            OnPropertyChanged(nameof(Enabled));
            return;
        }

        foreach (var poiGroup in PoiGroups)
        {
            poiGroup.SetEnabledNoRefresh(value);
        }

        OnPropertyChanged(nameof(Enabled));

        if (value)
        {
            PoiMessenger.Instance.Send(new PoiRefreshEvent(this));
        }
    }

    partial void OnEnabledChanged(bool value)
    {
        foreach (var poiGroup in PoiGroups)
        {
            poiGroup.SetEnabledNoRefresh(value);
        }

        PoiMessenger.Instance.Send(new PoiRefreshEvent(this));
    }

    public static PoiGroupCollection ForActors(IEnumerable<PoiGroup> actorPoiGroups, bool enabled = false)
    {
        return new PoiGroupCollection(enabled, TranslationManager.Get("map.actors"), PoiGroupKeys.Actors,
            actorPoiGroups,
            PoiGroupType.Actors);
    }

    public void Remove(List<IPoi> pois)
    {
        var removed = PoiGroups.Aggregate(false, (current, group) => group.Remove(pois) || current);

        if (removed)
        {
            OnPropertyChanged(nameof(Title));
        }
    }
}