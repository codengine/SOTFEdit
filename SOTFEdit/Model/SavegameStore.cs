using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;

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
        PlayerStateSaveData
    }

    private readonly string _path;
    private readonly ReaderWriterLockSlim _readerWriterLock = new();

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
        _readerWriterLock.EnterReadLock();
        try
        {
            var path = ResolvePath(fileType.GetFilename());
            return !File.Exists(path) ? default : JsonConverter.DeserializeFromFile<T>(path);
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
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
        _readerWriterLock.EnterWriteLock();
        try
        {
            var fullPath = ResolvePath(fileType.GetFilename());

            if (createBackup)
            {
                CreateBackup(fullPath);
            }

            JsonConverter.Serialize(fullPath, model);
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
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

    public string GetThumbPath()
    {
        var thumbPath = ResolvePath(FileType.SaveDataThumbnail.GetFilename());
        return File.Exists(thumbPath)
            ? thumbPath
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default_screenshot.png");
    }

    public void Delete(FileType fileType)
    {
        var path = ResolvePath(fileType.GetFilename());
        if (!File.Exists(path))
        {
            return;
        }

        File.Delete(path);
    }

    public void MoveToBackup(FileType fileType)
    {
        var path = ResolvePath(fileType.GetFilename());
        if (!File.Exists(path))
        {
            return;
        }

        var targetPath = GetFilenameForBackup(path);
        File.Move(path, targetPath);
    }

    public bool IsMultiplayer()
    {
        return !(new DirectoryInfo(_path).Parent?.Name.Equals("SinglePlayer") ?? false);
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