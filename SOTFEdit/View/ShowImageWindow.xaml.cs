using System.Windows;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class ShowImageWindow
{
    public ShowImageWindow(Window owner, string url, string title)
    {
        DataContext = new ShowImageWindowViewModel(url, title);
        Owner = owner;
        InitializeComponent();
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var desiredWidth = screenWidth * (2.0 / 3.0);
        var desiredHeight = screenHeight * (2.0 / 3.0);
        Title = title;
        Width = desiredWidth;
        Height = desiredHeight;
    }
}