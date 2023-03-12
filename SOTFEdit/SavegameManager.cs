using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;

namespace SOTFEdit;

public class SavegameManager : ObservableObject
{
    private readonly Dictionary<string, Savegame> _savegames = new();

    public IEnumerable<Savegame> Savegames => _savegames
        .OrderByDescending(savegame => savegame.Value.SavegameStore.LastWriteTime)
        .Select(pair => pair.Value)
        .ToList();

    public void LoadSavegames()
    {
        lock (this)
        {
            var savesPath = !string.IsNullOrWhiteSpace(Settings.Default.SavegamePath)
                ? Settings.Default.SavegamePath
                : GetSavegamePathFromAppData();

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

            _savegames.Clear();
            foreach (var savegame in FindSaveGames(savesPath))
            {
                _savegames.Add(savegame.Key, savegame.Value);
            }

            OnPropertyChanged(nameof(Savegames));
        }
    }

    private static Dictionary<string, Savegame> FindSaveGames(string savesPath)
    {
        var fileInfos = new DirectoryInfo(savesPath).GetFiles("SaveData.json", SearchOption.AllDirectories);
        return fileInfos.Select(file => CreateSaveInfo(file.Directory))
            .Where(savegame => savegame != null)
            .Select(savegame => savegame!)
            .ToDictionary(savegame => savegame.Id, savegame => savegame);
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

    public void Reload()
    {
        _savegames.Clear();
        LoadSavegames();
    }
}