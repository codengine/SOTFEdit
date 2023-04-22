using SOTFEdit.Model.Actors;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class ActorView
{
    public ActorView(ActorCollection actorCollection)
    {
        DataContext = new ActorViewModel(actorCollection);
        InitializeComponent();
    }
}