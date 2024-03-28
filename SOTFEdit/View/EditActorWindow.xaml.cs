using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class EditActorWindow
{
    public EditActorWindow(Window owner, Actor messageActor)
    {
        Owner = owner;
        SetupListeners();
        var allActorTypes = Ioc.Default.GetRequiredService<GameData>().ActorTypes.OrderBy(type => type.Name).ToList();
        allActorTypes.Insert(0, new EmptyActorType());
        DataContext = new EditActorViewModel(messageActor, allActorTypes);
        InitializeComponent();
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<RequestUpdateActorsEvent>(this,
            (_, message) => OnRequestUpdateActorsEvent(message));
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void OnRequestUpdateActorsEvent(RequestUpdateActorsEvent message)
    {
        Application.Current.Dispatcher.Invoke(Close);
        WeakReferenceMessenger.Default.Send(new UpdateActorsEvent(message.ViewModel));
    }

    private void EditActorWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}