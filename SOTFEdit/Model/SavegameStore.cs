using System;
using System.IO;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model;

public class SavegameStore
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public enum FileType
    {
        SaveDataThumbnail,
        GameSetupSaveData,
        PlayerInventorySaveData,
        PlayerArmourSystemSaveData,
        SaveData,
        GameStateSaveData,
        WorldObjectLocatorManagerSaveData,
        WeatherSystemSaveData,
        PlayerStateSaveData,
        ScrewStructureInstancesSaveData
    }

    private readonly string _path;

    public SavegameStore(string path)
    {
        _path = path;
        LastWriteTime = Directory.GetLastWriteTime(_path);
    }

    public DateTime LastWriteTime { get; }

    public JToken? LoadJsonRaw(FileType fileType)
    {
        return LoadJson<JToken>(fileType);
    }

    public T? LoadJson<T>(FileType fileType)
    {
        var path = ResolvePath(fileType.GetFilename());
        return !File.Exists(path) ? default : JsonConverter.DeserializeFromFile<T>(path);
    }

    internal string ResolvePath(string fileName)
    {
        return Path.Combine(_path, fileName);
    }

    public bool HasChanged()
    {
        return Directory.GetLastWriteTime(_path) != LastWriteTime;
    }

    public void StoreJson(FileType fileType, object model, bool createBackup)
    {
        var fullPath = ResolvePath(fileType.GetFilename());

        if (createBackup)
        {
            CreateBackup(fullPath);
        }

        JsonConverter.Serialize(fullPath, model);
    }

    private static void CreateBackup(string fullPath)
    {
        var backupPath = GetFilenameForBackup(fullPath);
        File.Copy(fullPath, backupPath);
    }

    private static string GetFilenameForBackup(string jsonPath)
    {
        var backupFn = jsonPath + ".bak";
        if (!File.Exists(backupFn))
        {
            return backupFn;
        }

        var i = 1;

        do
        {
            backupFn = jsonPath + $".bak{i++}";
        } while (File.Exists(backupFn));

        return backupFn;
    }

    public string? GetThumbPath()
    {
        var thumbPath = ResolvePath(FileType.SaveDataThumbnail.GetFilename());
        return File.Exists(thumbPath)
            ? thumbPath
            : null;
    }

    public DirectoryInfo? GetParentDirectory()
    {
        return new DirectoryInfo(_path).Parent;
    }

    public int DeleteBackups()
    {
        if (!Directory.Exists(_path))
        {
            return 0;
        }

        var fileInfos = new DirectoryInfo(_path).GetFiles("*.bak*", SearchOption.TopDirectoryOnly);
        foreach (var fileInfo in fileInfos)
        {
            Logger.Info($"Deleting backup {fileInfo.FullName}...");
        }

        return fileInfos.Length;
    }
}

public static class FileTypeExtensions
{
    public static string GetFilename(this SavegameStore.FileType fileType)
    {
        return fileType switch
        {
            SavegameStore.FileType.SaveDataThumbnail => fileType + ".png",
            _ => fileType + ".json"
        };
    }
}