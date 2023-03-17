using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for UserControl1.xaml
/// </summary>
public partial class ArmorPage
{
    public ArmorPage()
    {
        DataContext = Ioc.Default.GetRequiredService<ArmorPageViewModel>();
        InitializeComponent();
    }
}