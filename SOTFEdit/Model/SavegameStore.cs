using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Infrastructure;
using SearchOption = System.IO.SearchOption;

namespace SOTFEdit.Model;

public class SavegameStore
{
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

    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly string _path;

    public SavegameStore(string path)
    {
        _path = path;
        LastWriteTime = ReadLastWriteTime();
    }

    public DateTime LastWriteTime { get; private set; }

    private DateTime ReadLastWriteTime()
    {
        return Directory.GetLastWriteTime(_path);
    }

    public JToken? LoadJsonRaw(FileType fileType)
    {
        return LoadJson<JToken>(fileType);
    }

    public T? LoadJson<T>(FileType fileType)
    {
        var path = ResolvePath(fileType.GetFilename());
        return !File.Exists(path) ? default : JsonConverter.DeserializeFromFile<T>(path);
    }

    private string ResolvePath(string fileName)
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

        var countDeleted = 0;

        foreach (var fileType in Enum.GetValues<FileType>())
        {
            var backupFiles = GetBackupFiles(fileType.GetFilename());
            foreach (var backupFile in backupFiles)
            {
                Logger.Info($"Deleting backup {backupFile.FullName}...");
                FileSystem.DeleteFile(backupFile.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                countDeleted++;
            }
        }

        RereadLastWriteTime();

        return countDeleted;
    }

    private void RereadLastWriteTime()
    {
        LastWriteTime = ReadLastWriteTime();
    }

    private IEnumerable<FileInfo> GetBackupFiles(string prefix = "*")
    {
        return new DirectoryInfo(_path).GetFiles($"{prefix}.bak*", SearchOption.TopDirectoryOnly);
    }

    private FileInfo? GetBackupFile(FileType fileType, bool newestFile)
    {
        return (newestFile
            ? GetBackupFiles(fileType.GetFilename()).OrderByDescending(fileInfo => fileInfo.CreationTimeUtc)
            : GetBackupFiles(fileType.GetFilename()).OrderBy(fileInfo => fileInfo.CreationTimeUtc)).FirstOrDefault();
    }

    public int RestoreBackups(bool restoreFromNewest)
    {
        var restored = 0;
        foreach (var fileType in Enum.GetValues<FileType>())
        {
            if (GetBackupFile(fileType, restoreFromNewest) is not { } fileInfo)
            {
                continue;
            }

            RestoreBackup(fileType, fileInfo);
            restored++;
        }

        return restored;
    }

    private void RestoreBackup(FileType fileType, FileSystemInfo backupFile)
    {
        var originalFilePath = Path.Combine(_path, fileType.GetFilename());
        if (File.Exists(originalFilePath))
        {
            Logger.Info($"Deleting original file {originalFilePath}");
            FileSystem.DeleteFile(originalFilePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }

        Logger.Info($"Copying backup {backupFile.FullName} to {originalFilePath}");
        File.Copy(backupFile.FullName, originalFilePath);
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