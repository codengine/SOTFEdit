using SOTFEdit.Model.Storage;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View.Storage;

/// <summary>
///     Interaction logic for UserControl1.xaml
/// </summary>
public partial class AdvancedItemStorageUserControl
{
    public AdvancedItemStorageUserControl(AdvancedItemsStorage itemsStorage)
    {
        DataContext = new AdvancedItemStorageViewModel(itemsStorage);
        InitializeComponent();
    }
}