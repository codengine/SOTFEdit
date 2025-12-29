using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Events;

public class RequestUpdateActorsEvent(EditActorViewModel viewModel)
{
    public EditActorViewModel ViewModel { get; } = viewModel;
}