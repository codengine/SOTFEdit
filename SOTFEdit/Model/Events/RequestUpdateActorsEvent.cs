using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Events;

public class RequestUpdateActorsEvent
{
    public RequestUpdateActorsEvent(EditActorViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public EditActorViewModel ViewModel { get; }
}