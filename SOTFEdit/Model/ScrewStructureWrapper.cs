using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model;

public partial class ScrewStructureWrapper : ObservableObject
{
    private readonly ScrewStructure? _screwStructure;

    [ObservableProperty] private string? _modificationMode;

    public ScrewStructureWrapper(ScrewStructure? screwStructure, JToken token, int added, Position? position)
    {
        Token = token;
        Added = added;
        Position = position;
        _screwStructure = screwStructure;
    }

    public JToken Token { get; }
    public int Added { get; }
    public Position? Position { get; }

    public string Name => _screwStructure?.Name ?? "???";
    public string Category => _screwStructure?.Category ?? "Unknown";
    public int BuildCost => _screwStructure?.BuildCost ?? -1;

    private int PctDone => _screwStructure?.BuildCost is { } buildCost ? 100 * Added / buildCost : -1;
    public string PctDonePrintable => PctDone == -1 ? "???" : $"{PctDone}%";

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
}