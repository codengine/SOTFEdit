using SOTFEdit.Infrastructure;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class TranslationWindow : ICloseable
{
    public TranslationWindow()
    {
        InitializeComponent();
        DataContext = new TranslationViewModel(this);
    }
}