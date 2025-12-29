using System;

namespace SOTFEdit.Model.Map;

public class StaticAreaFilter(string name, Predicate<IPoi> predicate) : IAreaFilter
{
    public string Name { get; } = name;

    public bool ShouldInclude(IPoi poi)
    {
        return predicate.Invoke(poi);
    }
}