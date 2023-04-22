using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public partial class PoiGroup : ObservableObject, IPoiGrouper
{
    [ObservableProperty]
    private bool _enabled;

    public PoiGroup(bool enabled, IEnumerable<IPoi> pois, string title, BitmapImage? icon = null,
        PoiGroupType poiGroupType = PoiGroupType.Generic)
    {
        _enabled = enabled;
        Icon = icon;
        PoiGroupType = poiGroupType;
        Pois = new HashSet<IPoi>(pois);
        BaseTitle = title;
    }

    public HashSet<IPoi> Pois { get; }

    public int Count => Pois.Count;
    public BitmapImage? Icon { get; }
    public string BaseTitle { get; }
    public string Title => $"{BaseTitle} ({Count})";
    public PoiGroupType PoiGroupType { get; }

    public void SetEnabledNoRefresh(bool value)
    {
        foreach (var poi in Pois)
        {
            poi.Enabled = value;
        }

        if (_enabled == value)
        {
            return;
        }

        _enabled = value;
        OnPropertyChanged(nameof(Enabled));
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
}