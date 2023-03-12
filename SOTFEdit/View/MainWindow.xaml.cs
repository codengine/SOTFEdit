using System;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        SetupListeners();
        DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
        InitializeComponent();

        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        Title = $"{assemblyName.Name} v{assemblyName.Version?.Major}.{assemblyName.Version?.Minor}";
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SavegameStoredEvent>(this,
            (_, message) => { OnSavegameStored(message); });
        WeakReferenceMessenger.Default.Register<RequestRegrowTreesEvent>(this,
            (_, message) => { OnRequestRegrowTreesEvent(message); });
        WeakReferenceMessenger.Default.Register<RequestReviveFollowersEvent>(this,
            (_, message) => { OnRequestReviveFollowersEvent(message); });
        WeakReferenceMessenger.Default.Register<RequestSaveChangesEvent>(this,
            (_, message) => { OnRequestSaveChanges(message); });
    }

    private static void OnRequestSaveChanges(RequestSaveChangesEvent message)
    {
        if (message.SelectedSavegame.SavegameStore.HasChanged())
        {
            var overwriteResult =
                MessageBox.Show("The savegame has been modified outside. Do you really want to overwrite any changes?",
                    "Overwrite Changes", MessageBoxButton.YesNo);
            if (overwriteResult != MessageBoxResult.Yes)
            {
                return;
            }
        }

        message.InvokeCallback();
    }

    private static void OnRequestRegrowTreesEvent(RequestRegrowTreesEvent message)
    {
        WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(message.SelectedSavegame, message.BackupFiles,
            createBackup =>
            {
                message.SelectedSavegame.RegrowTrees(createBackup);
                WeakReferenceMessenger.Default.Send(new SavegameStoredEvent("Trees should now have regrown"));
            }));
    }

    private static void OnRequestReviveFollowersEvent(RequestReviveFollowersEvent message)
    {
        WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(message.SelectedSavegame, message.BackupFiles,
            createBackup =>
            {
                WeakReferenceMessenger.Default.Send(
                    message.SelectedSavegame.ReviveFollowers(createBackup)
                        ? new SavegameStoredEvent("Virginia and Kelvin should now be back again")
                        : new SavegameStoredEvent("Virginia and Kelvin should be alive already")
                );
            }));
    }

    private static void OnSavegameStored(SavegameStoredEvent message)
    {
        MessageBox.Show(message.Message);
    }

    protected override void OnContentRendered(EventArgs e)
    {
        Ioc.Default.GetRequiredService<SavegameManager>()
            .LoadSavegames();
    }
}