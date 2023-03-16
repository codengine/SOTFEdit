using System;
using System.IO;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
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
        Title =
            $"{assemblyName.Name} v{assemblyName.Version?.Major}.{assemblyName.Version?.Minor}.{assemblyName.Version?.Build}";
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
        WeakReferenceMessenger.Default.Register<SelectSavegameDirEvent>(this,
            (_, message) => { OnSelectSavegameDir(); });
    }

    private static void OnSelectSavegameDir()
    {
        var folderBrowser = new FolderPicker
        {
            Title = "Select Sons of the Forest \"Saves\" Directory"
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
        Ioc.Default.GetRequiredService<SavegameManager>().LoadSavegames();
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
                var countRegrown = message.SelectedSavegame.RegrowTrees(createBackup, message.VegetationStateSelected);
                var resultMessage =
                    countRegrown == 0
                        ? $"No trees to regrow found with state \"{message.VegetationStateSelected}\""
                        : $"{countRegrown} trees with previous state \"{message.VegetationStateSelected}\" should now have regrown";
                WeakReferenceMessenger.Default.Send(new SavegameStoredEvent(resultMessage, countRegrown > 0));
            }));
    }

    private static void OnRequestReviveFollowersEvent(RequestReviveFollowersEvent message)
    {
        WeakReferenceMessenger.Default.Send(new RequestSaveChangesEvent(message.SelectedSavegame, message.BackupFiles,
            createBackup =>
            {
                var hasChanges = message.SelectedSavegame.ReviveFollower(message.TypeId, createBackup);

                var actorName = message.TypeId == Constants.Actors.KelvinTypeId ? "Kelvin" : "Virginia";

                var resultMessage = hasChanges
                    ? $"{actorName} should now be back again"
                    : $"{actorName} should be alive already";

                WeakReferenceMessenger.Default.Send(new SavegameStoredEvent(resultMessage, hasChanges));
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