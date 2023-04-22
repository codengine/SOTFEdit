using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using NLog;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Savegame;

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
        ScrewStructureInstancesSaveData,
        PlayerClothingSystemSaveData,
        FiresSaveData,
        ScrewStructureNodeInstancesSaveData,
        StructureDestructionSaveData,
        WorldItemManagerSaveData,
        ZipLineManagerSaveData
    }

    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly string _path;

    private readonly Dictionary<FileType, SaveDataWrapper?> _rawData = new();
    private readonly ReaderWriterLockSlim _readerWriterLockSlim = new();

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

    public SaveDataWrapper? LoadJsonRaw(FileType fileType)
    {
        _readerWriterLockSlim.EnterUpgradeableReadLock();
        try
        {
            if (_rawData.TryGetValue(fileType, out var saveDataWrapper))
            {
                return saveDataWrapper;
            }

            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                if (_rawData.TryGetValue(fileType, out var wrapper))
                {
                    return wrapper;
                }

                var data = LoadJson<JToken>(fileType);
                _rawData[fileType] = data == null ? null : new SaveDataWrapper(data);
                return _rawData[fileType];
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }
        finally
        {
            _readerWriterLockSlim.ExitUpgradeableReadLock();
        }
    }

    private T? LoadJson<T>(FileType fileType)
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

    private void StoreJson(FileType fileType, object model)
    {
        var fullPath = ResolvePath(fileType.GetFilename());
        JsonConverter.Serialize(fullPath, model);
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

    public int DeleteBackups(ApplicationSettings.BackupFlag backupFlags)
    {
        var countDeleted = BackupManager.DeleteAllBackups(_path, backupFlags);
        RereadLastWriteTime();
        return countDeleted;
    }

    private void RereadLastWriteTime()
    {
        LastWriteTime = ReadLastWriteTime();
    }

    public int RestoreBackups(bool restoreFromNewest, ApplicationSettings.BackupFlag backupFlags)
    {
        return BackupManager.RestoreBackups(_path, restoreFromNewest, backupFlags);
    }

    public bool SaveAllModified(ApplicationSettings.BackupMode backupMode, ApplicationSettings.BackupFlag backupFlags)
    {
        var changedWrappers = _rawData.Where(kvp => kvp.Value != null && kvp.Value.HasModified())
            .Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value!))
            .ToList();

        if (changedWrappers.Count == 0)
        {
            Logger.Info("No changes");
            return false;
        }

        var hasBackupArchive = (backupFlags & ApplicationSettings.BackupFlag.TYPE_ARCHIVE) != 0 &&
                               backupMode != ApplicationSettings.BackupMode.None;
        if (hasBackupArchive)
        {
            BackupManager.BackupArchive(_path, backupMode);
        }

        var hasBackupSingleFile = (backupFlags & ApplicationSettings.BackupFlag.TYPE_SINGLEFILE) != 0 &&
                                  backupMode != ApplicationSettings.BackupMode.None;

        foreach (var (fileType, saveDataWrapper) in changedWrappers)
        {
            Logger.Info($"{fileType} is marked as modified, saving...");
            if (hasBackupSingleFile)
            {
                var fullPath = ResolvePath(fileType.GetFilename());
                BackupManager.BackupSingleFile(fullPath, backupMode);
            }

            saveDataWrapper.SerializeAllModified();
            StoreJson(fileType, saveDataWrapper.Parent);
        }

        return true;
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