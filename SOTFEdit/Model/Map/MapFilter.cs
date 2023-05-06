using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Map;

public partial class MapFilter : ObservableObject
{
    public enum RequirementsFilterType
    {
        All,
        AccessibleOnly,
        InaccessibleOnly
    }

    [ObservableProperty]
    private IAreaFilter _areaFilter = Map.AreaFilter.All;

    [ObservableProperty]
    private string? _fullText;

    [ObservableProperty]
    private RequirementsFilterType _requirementsFilter = RequirementsFilterType.All;

    [ObservableProperty]
    private bool _showOnlyUncollectedItems;

    public string? NormalizedLowercaseFullText;

    public MapFilter(AreaMaskManager areaManager)
    {
        var allAreas = areaManager.GetAllAreas();
        AreaFilterTypeValues = new List<IAreaFilter>
        {
            Map.AreaFilter.All, Map.AreaFilter.Surface, Map.AreaFilter.CavesOrBunkers
        };

        AreaFilterTypeValues.AddRange(allAreas.Where(area => !area.IsSurface()).OrderBy(area => area.Name)
            .Select(area => new AreaFilter(area)));
        RequirementsFilterTypeValues = Enum.GetValues(typeof(RequirementsFilterType)).Cast<RequirementsFilterType>()
            .Select(v =>
                new ComboBoxItemAndValue<RequirementsFilterType>(
                    TranslationManager.Get($"map.requirementsFilter.types.{v}"), v));
    }

    public List<IAreaFilter> AreaFilterTypeValues { get; }

    public IEnumerable<ComboBoxItemAndValue<RequirementsFilterType>> RequirementsFilterTypeValues { get; }

    partial void OnFullTextChanged(string? value)
    {
        NormalizedLowercaseFullText = value != null ? TranslationHelper.Normalize(value).ToLower() : null;
    }
}