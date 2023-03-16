using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
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
                .OrderByDescending(savegame => savegame.Value.LastSaveTime)
                .Select(pair => pair.Value)
                .ToList();
        }
        finally
        {
            _readerWriterLockSlim.ExitReadLock();
        }
    }

    public Savegame? ReloadSavegame(Savegame savegame)
    {
        var savesPath = !string.IsNullOrWhiteSpace(Settings.Default.SavegamePath)
            ? Settings.Default.SavegamePath
            : GetSavegamePathFromAppData();
        Logger.Info($"Detected savegame path: {savesPath}");

        _readerWriterLockSlim.EnterWriteLock();
        try
        {
            if (FindSaveGames(savesPath).TryGetValue(savegame.Title, out var foundSavegame))
            {
                _savegames[savegame.Title] = foundSavegame;
                return foundSavegame;
            }
        }
        finally
        {
            _readerWriterLockSlim.ExitWriteLock();
            OnPropertyChanged(nameof(Savegames));
        }

        return null;
    }

    public void LoadSavegames()
    {
        var savesPath = !string.IsNullOrWhiteSpace(Settings.Default.SavegamePath)
            ? Settings.Default.SavegamePath
            : GetSavegamePathFromAppData();
        Logger.Info($"Detected savegame path: {savesPath}");

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

    private static Dictionary<string, Savegame> FindSaveGames(string savesPath, string? titleFilter = null)
    {
        Logger.Info($"Reading savegames from {savesPath}");
        if (!Directory.Exists(savesPath))
        {
            return new Dictionary<string, Savegame>();
        }

        try
        {
            var fileInfos =
                new DirectoryInfo(savesPath).GetFiles("GameStateSaveData.json", SearchOption.AllDirectories);
            return fileInfos.Select(file => CreateSaveInfo(file.Directory))
                .Where(savegame => savegame != null)
                .Select(savegame => savegame!)
                .Where(savegame => titleFilter == null || titleFilter == savegame.Title)
                .ToDictionary(savegame => savegame.Title, savegame => savegame);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Unable to read savegames from {savesPath}");
        }

        return new Dictionary<string, Savegame>();
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