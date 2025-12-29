using System.Collections.Generic;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class ChangeScrewStructureTypeDialog : ICloseable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly MetroWindow _window;

    public ChangeScrewStructureTypeDialog(MetroWindow window, IReadOnlyList<ScrewStructure> screwStructures,
        ScrewStructureWrapper screwStructureWrapper)
    {
        _window = window;
        DataContext = new ChangeScrewStructureViewModel(this, screwStructures, screwStructureWrapper);
        InitializeComponent();
    }

    public void Close()
    {
        CloseAsync().Forget(ex => Logger.Error(ex, "Error while closing ChangeScrewStructureTypeDialog"));
    }

    private Task CloseAsync()
    {
        return _window.HideMetroDialogAsync(this, new MetroDialogSettings
        {
            AnimateHide = false
        });
    }
}