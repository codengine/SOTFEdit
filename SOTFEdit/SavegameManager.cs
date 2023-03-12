using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;

namespace SOTFEdit;

public class SavegameManager : ObservableObject
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<string, Savegame> _savegames = new();
    private readonly ReaderWriterLockSlim _readerWriterLockSlim = new();

    public IEnumerable<Savegame> Savegames => GetSavegameList();

    private IEnumerable<Savegame> GetSavegameList()
    {
        _readerWriterLockSlim.EnterReadLock();
        try
        {
            return _savegames
                .OrderByDescending(savegame => savegame.Value.SavegameStore.LastWriteTime)
                .Select(pair => pair.Value)
                .ToList();
        }
        finally
        {
            _readerWriterLockSlim.ExitReadLock();
        }
    }

    public void LoadSavegames()
    {
        var savesPath = !string.IsNullOrWhiteSpace(Settings.Default.SavegamePath)
            ? Settings.Default.SavegamePath
            : GetSavegamePathFromAppData();
        Logger.Info($"Detected savegame path: {savesPath}");

        if (!Directory.Exists(savesPath))
        {
            var folderBrowser = new FolderPicker
            {
                Title = "Select Sons of the Forest \"Saves\" Directory"
            };

            if (folderBrowser.ShowDialog() == true)
            {
                savesPath = folderBrowser.ResultPath;
            }
        }

        Settings.Default.SavegamePath = savesPath;
        Settings.Default.Save();

        _readerWriterLockSlim.EnterWriteLock();
        try
        {
            _savegames.Clear();
            foreach (var savegame in FindSaveGames(savesPath))
            {
                _savegames.Add(savegame.Key, savegame.Value);
            }
        }
        finally
        {
            _readerWriterLockSlim.ExitWriteLock();
        }

        OnPropertyChanged(nameof(Savegames));
    }

    private static Dictionary<string, Savegame> FindSaveGames(string savesPath)
    {
        var fileInfos = new DirectoryInfo(savesPath).GetFiles("SaveData.json", SearchOption.AllDirectories);
        return fileInfos.Select(file => CreateSaveInfo(file.Directory))
            .Where(savegame => savegame != null)
            .Select(savegame => savegame!)
            .ToDictionary(savegame => savegame.Title, savegame => savegame);
    }

    private static Savegame? CreateSaveInfo(DirectoryInfo? directory)
    {
        return directory is { Exists: true }
            ? new Savegame(directory.Name, new SavegameStore(directory.FullName))
            : null;
    }

    private static string GetSavegamePathFromAppData()
    {
        var localLowPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .Replace("Roaming", "LocalLow");
        return Path.Combine(localLowPath, "Endnight", "SonsOfTheForest", "Saves");
    }
}