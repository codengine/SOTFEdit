using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Map;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class MapSpawnActorsWindow : ICloseable
{
    public MapSpawnActorsWindow(Window owner, BasePoi destination)
    {
        Owner = owner;
        var allFamilyIds = Ioc.Default.GetRequiredService<NpcsPageViewModel>().AllActors
            .Where(actor => actor.FamilyId != null)
            .Select(actor => actor.FamilyId!.Value)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var allActorTypes = Ioc.Default.GetRequiredService<GameData>().ActorTypes.OrderBy(type => type.Name).ToList();

        DataContext = new MapSpawnActorsViewModel(this, destination, allFamilyIds, allActorTypes);
        InitializeComponent();
    }

    private void MapSpawnActorsWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}