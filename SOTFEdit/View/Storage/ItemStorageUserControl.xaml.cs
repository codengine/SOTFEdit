using SOTFEdit.Model.Storage;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View.Storage;

/// <summary>
///     Interaction logic for UserControl1.xaml
/// </summary>
public partial class ItemStorageUserControl
{
    public ItemStorageUserControl(BaseStorage itemsStorage)
    {
        DataContext = new ItemStorageViewModel(itemsStorage);
        InitializeComponent();
    }
}