using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit;

public class SavegameManager : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private static Savegame? _selectedSavegame;

    public static Savegame? SelectedSavegame
    {
        get => _selectedSavegame;
        set
        {
            _selectedSavegame = value;
            WeakReferenceMessenger.Default.Send(new SelectedSavegameChangedEvent(value));
        }
    }

    public static Dictionary<string, Savegame> GetSavegames()
    {
        var savesPath = GetSavePath();
        Logger.Info($"Detected savegame path: {savesPath}");
        return FindSaveGames(savesPath);
    }

    public static string GetSavePath()
    {
        var savesPath = !string.IsNullOrWhiteSpace(Settings.Default.SavegamePath)
            ? Settings.Default.SavegamePath
            : GetSavegamePathFromAppData();
        return savesPath;
    }

    private static Dictionary<string, Savegame> FindSaveGames(string? savesPath, string? idFilter = null)
    {
        Logger.Info($"Reading savegames from {savesPath}");
        if (!Directory.Exists(savesPath))
        {
            return new Dictionary<string, Savegame>();
        }

        try
        {
            var fileInfos =
                new DirectoryInfo(savesPath).GetFiles("SaveData.*", SearchOption.AllDirectories);
            return fileInfos.Select(file => CreateSaveInfo(file.Directory))
                .Where(savegame => savegame != null)
                .Select(savegame => savegame!)
                .Where(savegame => idFilter == null || idFilter == savegame.FullPath)
                .OrderByDescending(savegame => savegame.LastSaveTime)
                .DistinctBy(savegame => savegame.FullPath)
                .ToDictionary(savegame => savegame.FullPath, savegame => savegame);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Unable to read savegames from {savesPath}");
        }

        return new Dictionary<string, Savegame>();
    }

    public static Savegame? CreateSaveInfo(DirectoryInfo? directory)
    {
        return directory is { Exists: true }
            ? new Savegame(directory.FullName, directory.Name, new SavegameStore(directory.FullName))
            : null;
    }

    public static string GetSavegamePathFromAppData()
    {
        var localLowPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .Replace("Roaming", "LocalLow");
        return Path.Combine(localLowPath, "Endnight", "SonsOfTheForest", "Saves");
    }

    public static Savegame? ReloadSavegame(Savegame selectedSavegame)
    {
        var directoryInfo = new DirectoryInfo(selectedSavegame.FullPath);

        return !directoryInfo.Exists ? null : CreateSaveInfo(directoryInfo);
    }
}