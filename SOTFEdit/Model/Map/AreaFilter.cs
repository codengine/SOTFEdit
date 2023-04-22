using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Map;

public class AreaFilter : IAreaFilter
{
    public static readonly IAreaFilter All =
        new StaticAreaFilter(TranslationManager.Get("map.areaFilter.types.all"), _ => true);

    public static readonly IAreaFilter Surface =
        new StaticAreaFilter(TranslationManager.Get("map.areaFilter.types.surfaceOnly"), poi => !poi.IsUnderground);

    public static readonly IAreaFilter CavesOrBunkers =
        new StaticAreaFilter(TranslationManager.Get("map.areaFilter.types.undergroundOnly"), poi => poi.IsUnderground);

    private readonly Area _area;

    public AreaFilter(Area area)
    {
        _area = area;
    }

    public bool ShouldInclude(IPoi poi)
    {
        if (poi is not BasePoi basePoi)
        {
            return false;
        }

        return basePoi.Position?.Area.Equals(_area) ?? false;
    }

    public string Name => _area.Name;
}