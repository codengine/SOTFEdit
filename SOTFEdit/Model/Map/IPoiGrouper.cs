namespace SOTFEdit.Model.Map;

public interface IPoiGrouper
{
    public string Title { get; }
    public string BaseTitle { get; }
    public bool Enabled { get; }
    public PoiGroupType PoiGroupType { get; }
    public void SetEnabledNoRefresh(bool value);
}