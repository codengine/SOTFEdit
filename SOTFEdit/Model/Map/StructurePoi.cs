using System.Windows.Media.Imaging;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Map;

public class StructurePoi : BasePoi
{
    public StructurePoi(ScrewStructureWrapper screwStructureWrapper) : base(screwStructureWrapper.Position!)
    {
        ScrewStructureWrapper = screwStructureWrapper;
    }

    public ScrewStructureWrapper ScrewStructureWrapper { get; }

    public override string Title => ScrewStructureWrapper.Name;

    public override BitmapImage Icon => ScrewStructureWrapper.ScrewStructure?.Icon is { } icon
        ? $"/images/structures/{icon}".LoadAppLocalImage()
        : DefaultIcon;

    public override int IconWidth => 24;
    public override int IconHeight => 24;
    protected override int IconOffset => 12;

    public override void ApplyFilter(MapFilter mapFilter)
    {
        Filtered = ShouldFilter(mapFilter);
    }

    protected override bool ShouldFilter(MapFilter mapFilter)
    {
        return mapFilter.RequirementsFilter == MapFilter.RequirementsFilterType.InaccessibleOnly ||
               base.ShouldFilter(mapFilter);
    }
}