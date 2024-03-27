using System.Windows;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class ItemPlaterWindow : ICloseable
{
    public ItemPlaterWindow(Window owner, GameData gameData)
    {
        Owner = owner;
        DataContext = new ItemPlaterViewModel(this, gameData);

        InitializeComponent();
    }
}