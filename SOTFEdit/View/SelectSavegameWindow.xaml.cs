using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
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
        DataContext = Ioc.Default.GetRequiredService<SelectSavegameViewModel>();
        InitializeComponent();
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, _) => { Application.Current.Dispatcher.Invoke(Close); });
        WeakReferenceMessenger.Default.Register<RequestSelectSavegameDirEvent>(this,
            (_, _) => { OnRequestSelectSavegameDirEvent(); });
    }

    private static void OnRequestSelectSavegameDirEvent()
    {
        var folderBrowser = new FolderPicker
        {
            Title = "Select Sons of the Forest \"Saves\" Directory",
            InputPath = SavegameManager.GetSavePath()
        };

        if (folderBrowser.ShowDialog() != true)
        {
            return;
        }

        var savesPath = folderBrowser.ResultPath;
        if (string.IsNullOrWhiteSpace(savesPath) || !Directory.Exists(savesPath))
        {
            return;
        }

        Settings.Default.SavegamePath = savesPath;
        Settings.Default.Save();

        WeakReferenceMessenger.Default.Send(new SelectedSavegameDirChangedEvent(savesPath));
    }
}