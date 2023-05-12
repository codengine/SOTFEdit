using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public abstract partial class InformationalPoi : BasePoi
{
    private const string ScreenshotUrl =
        "https://raw.githubusercontent.com/codengine/SOTFEdit-Assets/main/screenshots/{0}";

    private readonly IEnumerable<Item>? _requirements;
    private readonly string? _screenshot;

    protected InformationalPoi(float x, float y, Position? teleport, string title, string? description,
        IEnumerable<Item>? requirements,
        string? screenshot, bool isUnderground = false, string? wikiLink = null) : base(x, y, teleport)
    {
        _requirements = requirements;
        Title = title;
        _screenshot = screenshot;
        Description = description;
        IsUnderground = teleport?.Area.IsUnderground() ?? isUnderground;
        WikiLink = wikiLink;
    }

    public string? WikiLink { get; }

    protected int[]? MissingRequiredItems { get; init; }

    public string? ScreenshotSmall => _screenshot != null
        ? GetScreenshotPath(_screenshot.ExtendFilenameWith("_tn"))
        : null;

    public IEnumerable<ItemInInventoryWrapper> Requirements => GetRequirements();

    public override string Title { get; }

    public override bool IsUnderground { get; }

    public bool HasAnyRequirements => _requirements != null && _requirements.Any();

    protected virtual string GetScreenshotPath(string screenshot)
    {
        return string.Format(ScreenshotUrl, screenshot);
    }

    private IEnumerable<ItemInInventoryWrapper> GetRequirements()
    {
        if (_requirements == null || !_requirements.Any())
        {
            return Enumerable.Empty<ItemInInventoryWrapper>();
        }

        return _requirements.Select(item => new ItemInInventoryWrapper(item, HasRequiredItem(item))).ToList();
    }

    private bool HasRequiredItem(Item item)
    {
        return MissingRequiredItems == null || !MissingRequiredItems.Any() || !MissingRequiredItems.Contains(item.Id);
    }

    public override void ApplyFilter(MapFilter mapFilter)
    {
        Filtered = ShouldFilter(mapFilter);
    }

    protected override bool ShouldFilter(MapFilter mapFilter)
    {
        return (mapFilter.RequirementsFilter is var requirementsFilter &&
                ShouldFilterByRequirements(requirementsFilter)) || base.ShouldFilter(mapFilter);
    }

    private bool ShouldFilterByRequirements(MapFilter.RequirementsFilterType requirementsFilter)
    {
        return requirementsFilter switch
        {
            MapFilter.RequirementsFilterType.All => false,
            MapFilter.RequirementsFilterType.InaccessibleOnly => _requirements == null || !_requirements.Any() ||
                                                                 HasRequiredItems(),
            MapFilter.RequirementsFilterType.AccessibleOnly => !HasRequiredItems(),
            _ => false
        };
    }

    private bool HasRequiredItems()
    {
        return MissingRequiredItems == null || MissingRequiredItems.Length == 0;
    }

    [RelayCommand]
    private void OpenWiki()
    {
        if (WikiLink != null)
        {
            WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForUrl(WikiLink));
        }
    }

    [RelayCommand]
    private void ShowScreenshot()
    {
        if (_screenshot is null)
        {
            return;
        }

        var title = Title + (Description != null ? " - " + Description : "");
        WeakReferenceMessenger.Default.Send(new ShowMapImageEvent(GetScreenshotPath(_screenshot), title));
    }
}