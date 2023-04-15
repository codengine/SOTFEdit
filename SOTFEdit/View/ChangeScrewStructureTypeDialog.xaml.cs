using System.Collections.Generic;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class ChangeScrewStructureTypeDialog : ICloseable
{
    private readonly MetroWindow _window;

    public ChangeScrewStructureTypeDialog(MetroWindow window, IReadOnlyList<ScrewStructure> screwStructures,
        ScrewStructureWrapper screwStructureWrapper)
    {
        _window = window;
        DataContext = new ChangeScrewStructureViewModel(this, screwStructures, screwStructureWrapper);
        InitializeComponent();
    }

    public async void Close()
    {
        await _window.HideMetroDialogAsync(this, new MetroDialogSettings
        {
            AnimateHide = false
        });
    }
}