using System.Windows;
using System.Windows.Input;
using SOTFEdit.Infrastructure;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class RegrowTreesWindow : ICloseable
{
    public RegrowTreesWindow(Window owner)
    {
        Owner = owner;
        DataContext = new RegrowTreesViewModel(this);
        InitializeComponent();
    }

    private void RegrowTreesWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}