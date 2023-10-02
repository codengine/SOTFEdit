using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Events;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for Window1.xaml
/// </summary>
public partial class SelectSavegameWindow
{
    public SelectSavegameWindow()
    {
        SetupListeners();
        DataContext = new SelectSavegameViewModel();
        InitializeComponent();
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, _) => Application.Current.Dispatcher.Invoke(Close));
        WeakReferenceMessenger.Default.Register<RequestSelectSavegameDirEvent>(this,
            (_, _) => OnRequestSelectSavegameDirEvent());
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        WeakReferenceMessenger.Default.UnregisterAll(DataContext);
    }

    private void OnRequestSelectSavegameDirEvent()
    {
        var savePath = SavegameManager.GetSavePath();

        var folderBrowser = new FolderPicker
        {
            Title = TranslationManager.Get("windows.selectSavegame.folderBrowserTitle"),
            InputPath = Directory.Exists(savePath) ? savePath : null
        };

        if (folderBrowser.ShowDialog(this) != true)
        {
            return;
        }

        var savesPath = folderBrowser.ResultPath;
        if (string.IsNullOrWhiteSpace(savesPath) || !Directory.Exists(savesPath))
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new SelectedSavegameDirChangedEvent(savesPath));
    }

    private void SelectSavegameWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}