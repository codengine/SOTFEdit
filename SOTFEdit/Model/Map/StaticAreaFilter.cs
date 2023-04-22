using System;

namespace SOTFEdit.Model.Map;

public class StaticAreaFilter : IAreaFilter
{
    private readonly Predicate<IPoi> _predicate;

    public StaticAreaFilter(string name, Predicate<IPoi> predicate)
    {
        _predicate = predicate;
        Name = name;
    }

    public string Name { get; }

    public bool ShouldInclude(IPoi poi)
    {
        return _predicate.Invoke(poi);
    }
}