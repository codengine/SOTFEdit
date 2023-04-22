namespace SOTFEdit.Model.Map;

public interface IAreaFilter
{
    // ReSharper disable once UnusedMemberInSuper.Global
    public string Name { get; }
    public bool ShouldInclude(IPoi poi);
}