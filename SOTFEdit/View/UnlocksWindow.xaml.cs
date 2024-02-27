using System.Windows;
using SOTFEdit.Infrastructure;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class UnlocksWindow : ICloseable
{
    public UnlocksWindow(Window owner, string playerProfilePath)
    {
        Owner = owner;
        DataContext = new UnlocksViewModel(playerProfilePath, this);

        InitializeComponent();
    }
}