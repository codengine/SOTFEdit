namespace SOTFEdit.Model.Events;

public class ChangeScrewStructureResult(
    ScrewStructureWrapper screwStructureWrapper,
    ScrewStructure? selectedScrewStructure)
{
    public ScrewStructureWrapper ScrewStructureWrapper { get; } = screwStructureWrapper;
    public ScrewStructure? SelectedScrewStructure { get; } = selectedScrewStructure;
}