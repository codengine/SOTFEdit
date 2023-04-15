namespace SOTFEdit.Model.Events;

public class ChangeScrewStructureResult
{
    public ChangeScrewStructureResult(ScrewStructureWrapper screwStructureWrapper,
        ScrewStructure? selectedScrewStructure)
    {
        ScrewStructureWrapper = screwStructureWrapper;
        SelectedScrewStructure = selectedScrewStructure;
    }

    public ScrewStructureWrapper ScrewStructureWrapper { get; }
    public ScrewStructure? SelectedScrewStructure { get; }
}