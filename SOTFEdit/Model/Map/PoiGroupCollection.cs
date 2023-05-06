using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public partial class PoiGroupCollection : ObservableObject, IPoiGrouper
{
    [ObservableProperty]
    private bool _enabled;

    public PoiGroupCollection(bool enabled, string title, IEnumerable<PoiGroup> groups,
        PoiGroupType poiGroupType = PoiGroupType.Generic)
    {
        _enabled = enabled;
        PoiGroupType = poiGroupType;
        PoiGroups = groups;
        BaseTitle = title;
    }

    public IEnumerable<PoiGroup> PoiGroups { get; }

    private int Count => PoiGroups.Select(group => group.Count).Sum();

    public void SetEnabledNoRefresh(bool value)
    {
        _enabled = value;
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

    public string Title => $"{BaseTitle} ({Count})";
    public string BaseTitle { get; }
    public PoiGroupType PoiGroupType { get; }

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
        return new PoiGroupCollection(enabled, TranslationManager.Get("map.actors"), actorPoiGroups,
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