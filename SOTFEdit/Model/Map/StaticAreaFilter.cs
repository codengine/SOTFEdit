using System;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Map;

public class StaticAreaFilter : IAreaFilter
{
    private readonly Predicate<IPoi> _predicate;
    private readonly string _translationKey;

    public StaticAreaFilter(string translationKey, Predicate<IPoi> predicate)
    {
        _predicate = predicate;
        _translationKey = translationKey;
    }

    public string Name => TranslationManager.Get(_translationKey);

    public bool ShouldInclude(IPoi poi)
    {
        return _predicate.Invoke(poi);
    }

    public override string ToString()
    {
        return Name;
    }
}