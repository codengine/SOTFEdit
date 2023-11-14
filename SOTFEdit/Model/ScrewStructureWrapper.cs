using System;
using System.Collections.Immutable;
using System.Linq;
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
    [ObservableProperty]
    private int _added;

    private ScrewStructureModificationMode _modificationMode = ScrewStructureModificationMode.None;

    [ObservableProperty]
    private ScrewStructure? _screwStructure;

    public ScrewStructureWrapper(ScrewStructure? screwStructure, JToken token, int added, Position? position,
        ScrewStructureOrigin origin, int index)
    {
        Token = token;
        Added = added;
        Position = position;
        Origin = origin;
        Index = index;
        ScrewStructure = screwStructure;
        var canFinish = screwStructure?.CanFinish ?? false;
        var canEdit = screwStructure?.CanEdit ?? false;
        ModificationModes = GetModificationModes(origin, canFinish, canEdit);
        PctDone = ScrewStructure?.BuildCost is { } buildCost ? buildCost == 0 ? 100 : 100 * Added / buildCost : -1;
    }

    public ScrewStructureModificationMode? ModificationMode
    {
        get => _modificationMode;
        set
        {
            if (value is not { } modificationMode)
            {
                SetProperty(ref _modificationMode, ScrewStructureModificationMode.None);
                return;
            }

            if (ModificationModes.Contains(modificationMode))
            {
                SetProperty(ref _modificationMode, modificationMode);
            }
        }
    }

    public ImmutableSortedSet<ScrewStructureModificationMode> ModificationModes { get; }

    public JToken Token { get; }
    public Position? Position { get; }
    public ScrewStructureOrigin Origin { get; }
    public int Index { get; }

    public string Name => ScrewStructure?.Name ?? "???";
    public string Category => ScrewStructure?.CategoryName ?? TranslationManager.Get("generic.unknown");
    public int BuildCost => ScrewStructure?.BuildCost ?? -1;

    // ReSharper disable once MemberCanBePrivate.Global
    public int PctDone { get; }

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

    private static ImmutableSortedSet<ScrewStructureModificationMode> GetModificationModes(ScrewStructureOrigin origin,
        bool canFinish, bool canEdit)
    {
        return Enum.GetValues<ScrewStructureModificationMode>()
            .Where(mode => IsModeAllowed(mode, origin, canFinish, canEdit))
            .ToImmutableSortedSet();
    }

    private static bool IsModeAllowed(ScrewStructureModificationMode mode, ScrewStructureOrigin origin,
        bool canFinish,
        bool canEdit)
    {
        if (!canEdit)
        {
            return mode == ScrewStructureModificationMode.None;
        }

        switch (mode)
        {
            case ScrewStructureModificationMode.None:
            case ScrewStructureModificationMode.Remove:
                return true;
            case ScrewStructureModificationMode.AlmostFinish:
            case ScrewStructureModificationMode.Unfinish:
                return origin == ScrewStructureOrigin.Unfinished || canFinish;
            case ScrewStructureModificationMode.Finish:
                return origin == ScrewStructureOrigin.Unfinished && canFinish;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
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

    private bool CanChangeType()
    {
        return Origin == ScrewStructureOrigin.Unfinished || (_screwStructure?.CanFinish ?? false);
    }

    [RelayCommand(CanExecute = nameof(CanChangeType))]
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
        ModificationMode = ScrewStructureModificationMode.AlmostFinish;
        ChangedTypeId = selectedScrewStructure.Id;
        OnPropertyChanged(nameof(BuildCost));
    }
}