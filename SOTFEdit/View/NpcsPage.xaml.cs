using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class NpcsPage
{
    public NpcsPage(NpcsPageViewModel pageViewModel)
    {
        DataContext = pageViewModel;
        InitializeComponent();
    }
}