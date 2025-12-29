using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Map;

public class AreaFilter(Area area) : IAreaFilter
{
    public static readonly IAreaFilter All =
        new StaticAreaFilter(TranslationManager.Get("map.areaFilter.types.all"), _ => true);

    public static readonly IAreaFilter Surface =
        new StaticAreaFilter(TranslationManager.Get("map.areaFilter.types.surfaceOnly"), poi => !poi.IsUnderground);

    public static readonly IAreaFilter CavesOrBunkers =
        new StaticAreaFilter(TranslationManager.Get("map.areaFilter.types.undergroundOnly"), poi => poi.IsUnderground);

    public bool ShouldInclude(IPoi poi)
    {
        if (poi is not BasePoi basePoi)
        {
            return false;
        }

        return basePoi.Position?.Area.Equals(area) ?? false;
    }

    public string Name => area.Name;
}