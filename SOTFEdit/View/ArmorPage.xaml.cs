using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for UserControl1.xaml
/// </summary>
public partial class ArmorPage
{
    public ArmorPage()
    {
        InitializeComponent();
    }

    public void Update(Savegame savegame, bool createBackup)
    {
        ((ArmorPageViewModel)DataContext).Update(savegame, createBackup);
    }
}