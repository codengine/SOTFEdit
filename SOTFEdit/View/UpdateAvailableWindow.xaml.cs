using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using SOTFEdit.Model.Events;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class UpdateAvailableWindow
{
    private readonly string? _link;

    public UpdateAvailableWindow(Window owner, VersionCheckResultEvent message)
    {
        _link = message.Link;
        DataContext = new UpdateAvailableViewModel(message.Changelog, message.LatestTagVersion);
        Owner = owner;
        InitializeComponent();
    }

    private void Download_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = _link,
            UseShellExecute = true
        });
        Close();
    }

    private void Ignore_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void UpdateAvailableWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}