using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
using SOTFEdit.Model;

namespace SOTFEdit;

public class SavegameManager : ObservableObject
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public Dictionary<string, Savegame> GetSavegames()
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
                new DirectoryInfo(savesPath).GetFiles("GameStateSaveData.json", SearchOption.AllDirectories);
            return fileInfos.Select(file => CreateSaveInfo(file.Directory))
                .Where(savegame => savegame != null)
                .Select(savegame => savegame!)
                .Where(savegame => idFilter == null || idFilter == savegame.FullPath)
                .OrderByDescending(savegame => savegame.LastSaveTime)
                .ToDictionary(savegame => savegame.FullPath, savegame => savegame);
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
            ? new Savegame(directory.FullName, directory.Name, new SavegameStore(directory.FullName))
            : null;
    }

    private static string GetSavegamePathFromAppData()
    {
        var localLowPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .Replace("Roaming", "LocalLow");
        return Path.Combine(localLowPath, "Endnight", "SonsOfTheForest", "Saves");
    }

    public Savegame? ReloadSavegame(Savegame selectedSavegame)
    {
        var directoryInfo = new DirectoryInfo(selectedSavegame.FullPath);

        return !directoryInfo.Exists ? null : CreateSaveInfo(directoryInfo);
    }
}