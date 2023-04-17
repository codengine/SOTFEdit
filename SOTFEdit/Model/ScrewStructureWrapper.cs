using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Events;
using SOTFEdit.View;

namespace SOTFEdit.Model;

public partial class ScrewStructureWrapper : ObservableObject
{
    [ObservableProperty] private int _added;

    [ObservableProperty] private ScrewStructureModificationMode? _modificationMode;

    [ObservableProperty] private ScrewStructure? _screwStructure;

    public ScrewStructureWrapper(ScrewStructure? screwStructure, JToken token, int added, Position? position)
    {
        Token = token;
        Added = added;
        Position = position;
        ScrewStructure = screwStructure;
    }

    public JToken Token { get; }
    public Position? Position { get; }

    public string Name => ScrewStructure?.Name ?? "???";
    public string Category => ScrewStructure?.CategoryName ?? TranslationManager.Get("generic.unknown");
    public int BuildCost => ScrewStructure?.BuildCost ?? -1;

    private int PctDone => ScrewStructure?.BuildCost is { } buildCost ? 100 * Added / buildCost : -1;
    public string PctDonePrintable => PctDone == -1 ? "???" : $"{PctDone}%";

    public int? ChangedTypeId { get; private set; }

    public Color PctDoneColor
    {
        get
        {
            return PctDone switch
            {
                -1 => Colors.Aqua,
                <= 40 => Colors.Red,
                <= 60 => Colors.Orange,
                <= 80 => Colors.Yellow,
                _ => Colors.LawnGreen
            };
        }
    }

    [RelayCommand]
    private void OpenMapAtStructurePos()
    {
        if (Position != null)
        {
            WeakReferenceMessenger.Default.Send(new ZoomToPosEvent(Position));
        }
    }

    [RelayCommand]
    private void ChangeType()
    {
        var screwStructures = Ioc.Default.GetRequiredService<GameData>().ScrewStructures;
        WeakReferenceMessenger.Default.Send(new ShowDialogEvent(window =>
            new ChangeScrewStructureTypeDialog(window, screwStructures, this)));
    }

    public void Update(ScrewStructure? selectedScrewStructure)
    {
        if (selectedScrewStructure == null || selectedScrewStructure.Id == ScrewStructure?.Id)
        {
            return;
        }

        ScrewStructure = selectedScrewStructure;
        Added = ScrewStructure?.BuildCost - 1 ?? 0;
        ModificationMode = ScrewStructureModificationMode.Finish;
        ChangedTypeId = selectedScrewStructure.Id;
        OnPropertyChanged(nameof(BuildCost));
    }
}