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
    private bool _hideCompleted = Settings.Default.MapFilterHideCompleted;

    public string? NormalizedLowercaseFullText;

    public MapFilter(AreaMaskManager areaManager)
    {
        var allAreas = areaManager.GetAllAreas();
        AreaFilterTypeValues =
        [
            Map.AreaFilter.All, Map.AreaFilter.Surface, Map.AreaFilter.CavesOrBunkers,
            .. allAreas.Where(area => !area.IsSurface()).OrderBy(area => area.Name)
                .Select(area => new AreaFilter(area)),
        ];
        RequirementsFilterTypeValues = Enum.GetValues(typeof(RequirementsFilterType)).Cast<RequirementsFilterType>()
            .Select(v =>
                new ComboBoxItemAndValue<RequirementsFilterType>(
                    TranslationManager.Get($"map.requirementsFilter.types.{v}"), v));

        // Load persisted filter values
        var areaFilterSetting = Settings.Default.MapFilterAreaFilter;
        if (!string.IsNullOrWhiteSpace(areaFilterSetting))
        {
            var match = AreaFilterTypeValues.FirstOrDefault(a => a.ToString() == areaFilterSetting);
            if (match != null)
                _areaFilter = match;
        }
        _fullText = Settings.Default.MapFilterFullText;
        if (Enum.TryParse(
            Settings.Default.MapFilterRequirementsFilter, out RequirementsFilterType req))
            _requirementsFilter = req;
        _hideCompleted = Settings.Default.MapFilterHideCompleted;
    }

    public List<IAreaFilter> AreaFilterTypeValues { get; }

    public IEnumerable<ComboBoxItemAndValue<RequirementsFilterType>> RequirementsFilterTypeValues { get; }

    partial void OnAreaFilterChanged(IAreaFilter value)
    {
        Settings.Default.MapFilterAreaFilter = value?.ToString();
        Settings.Default.Save();
    }

    partial void OnFullTextChanged(string? value)
    {
        NormalizedLowercaseFullText = value != null ? TranslationHelper.Normalize(value).ToLower() : null;
        Settings.Default.MapFilterFullText = value;
        Settings.Default.Save();
    }

    partial void OnRequirementsFilterChanged(RequirementsFilterType value)
    {
        Settings.Default.MapFilterRequirementsFilter = value.ToString();
        Settings.Default.Save();
    }

    partial void OnHideCompletedChanged(bool value)
    {
        Settings.Default.MapFilterHideCompleted = value;
        Settings.Default.Save();
    }
}