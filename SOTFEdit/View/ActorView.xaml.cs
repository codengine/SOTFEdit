using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class ActorView
{
    private readonly Savegame _selectedSavegame;

    public ActorView(ActorCollection actorCollection, Savegame selectedSavegame)
    {
        _selectedSavegame = selectedSavegame;
        SetupListeners();
        DataContext = new ActorViewModel(actorCollection);
        InitializeComponent();
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<UpdateActorsEvent>(this,
            (_, message) => { OnUpdateActorsEvent(message); });
    }

    private void OnUpdateActorsEvent(UpdateActorsEvent message)
    {
        Ioc.Default.GetRequiredService<ActorModifier>().Modify(_selectedSavegame, message);
        WeakReferenceMessenger.Default.Send(new JsonModelChangedEvent(SavegameStore.FileType.SaveData));
    }
}