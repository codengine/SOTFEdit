using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        SaveDataArchive,
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
        ZipLineManagerSaveData,
        ScrewTrapsSaveData,
        ConstructionsSaveData
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
        var fileName = fileType.GetFilename();
        var path = ResolvePath(fileName);
        return File.Exists(path)
            ? JsonConverter.DeserializeFromFile<T>(path)
            : LoadJsonFromArchiveIfExists<T>(fileName);
    }

    private T? LoadJsonFromArchiveIfExists<T>(string fileName)
    {
        var archivePath = ResolvePath(FileType.SaveDataArchive.GetFilename());
        if (!File.Exists(archivePath))
        {
            return default;
        }

        using var archive = ZipFile.OpenRead(archivePath);
        var fileEntry = archive.GetEntry(fileName);
        if (fileEntry == null)
        {
            return default;
        }

        using var stream = fileEntry.Open();
        using var streamReader = new StreamReader(stream);
        var json = streamReader.ReadToEnd();
        return JsonConverter.DeserializeFromString<T>(json);
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
        var fileName = fileType.GetFilename();
        var fullPath = ResolvePath(fileName);
        if (File.Exists(fullPath))
        {
            JsonConverter.Serialize(fullPath, model);
        }
        else
        {
            StoreJsonToArchive(fileName, model);
        }
    }

    private void StoreJsonToArchive(string fileName, object model)
    {
        var archivePath = ResolvePath(FileType.SaveDataArchive.GetFilename());
        if (!File.Exists(archivePath))
        {
            return;
        }

        using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Update);
        archive.GetEntry(fileName)?.Delete();

        var zipEntry = archive.CreateEntry(fileName);

        var json = JsonConverter.Serialize(model);
        using var stream = zipEntry.Open();
        using var streamWriter = new StreamWriter(stream);
        streamWriter.Write(json);
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
        var countDeleted = BackupManager.DeleteAllBackups(_path);
        RereadLastWriteTime();
        return countDeleted;
    }

    private void RereadLastWriteTime()
    {
        LastWriteTime = ReadLastWriteTime();
    }

    public int RestoreBackups(bool restoreFromNewest)
    {
        return BackupManager.RestoreBackups(_path, restoreFromNewest);
    }

    public bool SaveAllModified(ApplicationSettings.BackupMode backupMode)
    {
        var changedWrappers = _rawData.Where(kvp => kvp.Value != null && kvp.Value.HasModified())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!);

        if (changedWrappers.Count == 0)
        {
            Logger.Info("No changes");
            return false;
        }

        SaveWrappers(backupMode, changedWrappers);

        return true;
    }

    public void SaveWrappers(ApplicationSettings.BackupMode backupMode,
        Dictionary<FileType, SaveDataWrapper> changedWrappers)
    {
        if (backupMode != ApplicationSettings.BackupMode.None)
        {
            BackupManager.BackupArchive(_path, backupMode);
        }

        foreach (var (fileType, saveDataWrapper) in changedWrappers)
        {
            Logger.Info($"{fileType} is marked as modified, saving...");
            saveDataWrapper.SerializeAllModified();
            StoreJson(fileType, saveDataWrapper.Parent);
        }
    }
}

public static class FileTypeExtensions
{
    public static string GetFilename(this SavegameStore.FileType fileType)
    {
        return fileType switch
        {
            SavegameStore.FileType.SaveDataThumbnail => fileType + ".png",
            SavegameStore.FileType.SaveDataArchive => "SaveData.zip",
            _ => fileType + ".json"
        };
    }
}