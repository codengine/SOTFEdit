using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SOTFEdit.View;

[ObservableObject]
public partial class SpawnFollowerInputDialog
{
    [ObservableProperty] private int? _count = 1;
    [ObservableProperty] private int _max = 1;

    public SpawnFollowerInputDialog(Window owner)
    {
        DataContext = this;
        Owner = owner;
        InitializeComponent();
    }

    [RelayCommand]
    private void Ok()
    {
        DialogResult = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        Close();
    }
}