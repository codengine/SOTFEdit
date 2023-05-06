using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model.Events;

namespace SOTFEdit.View;

[ObservableObject]
public partial class AboutWindow
{
    public AboutWindow(Window parent)
    {
        App.GetAssemblyVersion(out _, out var assemblyVersion);
        AssemblyVersion = $"v{assemblyVersion}";
        Owner = parent;
        DataContext = this;
        InitializeComponent();
    }

    public string AssemblyVersion { get; }

    [RelayCommand]
    private static void OpenDiscord()
    {
        WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForUrl("https://discord.gg/867UDYvvqE"));
    }

    [RelayCommand]
    private void CloseWindow()
    {
        Close();
    }

    private void AboutWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}