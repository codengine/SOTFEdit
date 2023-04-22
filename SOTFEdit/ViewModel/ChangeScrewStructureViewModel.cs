using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;

namespace SOTFEdit.ViewModel;

public partial class ChangeScrewStructureViewModel : ObservableObject
{
    private readonly ICloseable _parent;
    private readonly ScrewStructureWrapper _screwStructureWrapper;

    public ChangeScrewStructureViewModel(ICloseable parent, IReadOnlyList<ScrewStructure> screwStructures,
        ScrewStructureWrapper screwStructureWrapper)
    {
        _parent = parent;
        _screwStructureWrapper = screwStructureWrapper;
        ScrewStructures = CollectionViewSource.GetDefaultView(screwStructures
            .OrderBy(screwStructure => screwStructure.CategoryName)
            .ThenBy(screwStructure => screwStructure.Name).ToList());
        ScrewStructures.GroupDescriptions.Add(new PropertyGroupDescription("CategoryName"));
        SelectedScrewStructure =
            screwStructures.FirstOrDefault(structure => structure.Id == screwStructureWrapper.ScrewStructure?.Id);
    }

    public ICollectionView ScrewStructures { get; }
    public ScrewStructure? SelectedScrewStructure { get; set; }

    [RelayCommand]
    private void Cancel()
    {
        _parent.Close();
    }

    [RelayCommand]
    private void Ok()
    {
        WeakReferenceMessenger.Default.Send(new ChangeScrewStructureResult(_screwStructureWrapper,
            SelectedScrewStructure));
        _parent.Close();
    }
}