using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using NLog;
using SearchOption = System.IO.SearchOption;

namespace SOTFEdit.Model.Savegame;

public static class BackupManager
{
    private const string ArchiveBaseFn = "SOTFEdit";
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public static int DeleteAllBackups(string path)
    {
        if (!Directory.Exists(path))
        {
            return 0;
        }

        var archiveFiles = GetBackupArchiveFiles(path);
        var countDeleted = 0;
        foreach (var archiveFile in archiveFiles)
        {
            Logger.Info($"Deleting backup {archiveFile.FullName}...");
            DeleteFile(archiveFile.FullName);
            countDeleted++;
        }

        return countDeleted;
    }

    private static void DeleteFile(string path)
    {
        FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
    }

    private static IEnumerable<FileInfo> GetBackupArchiveFiles(string path)
    {
        return new DirectoryInfo(path).GetFiles($"{ArchiveBaseFn}*.zip", SearchOption.TopDirectoryOnly);
    }

    private static FileInfo? GetBackupArchive(string path, bool newestFile)
    {
        return (newestFile
                ? GetBackupArchiveFiles(path).OrderByDescending(fileInfo => fileInfo.LastWriteTime)
                : GetBackupArchiveFiles(path).OrderBy(fileInfo => fileInfo.LastWriteTime))
            .FirstOrDefault();
    }

    public static int RestoreBackups(string path, bool restoreFromNewest)
    {
        var backupArchive = GetBackupArchive(path, restoreFromNewest);
        if (backupArchive == null)
        {
            Logger.Info("No backup found");
            return 0;
        }

        var archivePath = Path.Combine(path, SavegameStore.FileType.SaveDataArchive.GetFilename());
        if (File.Exists(archivePath))
        {
            Logger.Info("SaveData archive found, overwrite from backup...");
            DeleteFile(archivePath);
            File.Copy(backupArchive.FullName, archivePath);
        }
        else
        {
            Logger.Info($"Extracting backups from {backupArchive.FullName} to {path}");
            var archive = ZipFile.OpenRead(backupArchive.FullName);
            archive.ExtractToDirectory(path, true);
        }

        return 1;
    }

    public static void BackupArchive(string dirPath, ApplicationSettings.BackupMode backupMode)
    {
        switch (backupMode)
        {
            case ApplicationSettings.BackupMode.None:
                return;
            case ApplicationSettings.BackupMode.Default:
                CreateDefaultArchiveBackup(dirPath);
                break;
            case ApplicationSettings.BackupMode.One:
                CreateArchiveOneBackup(dirPath);
                break;
            case ApplicationSettings.BackupMode.InitialAndOne:
                CreateArchiveInitialAndOneBackup(dirPath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(backupMode), backupMode, null);
        }
    }

    private static void CreateArchiveInitialAndOneBackup(string dirPath)
    {
        var dirInfo = new DirectoryInfo(dirPath);
        var baseFn = GetBaseArchiveFn(dirInfo);
        var fn = $"{baseFn}.zip";
        if (File.Exists(Path.Combine(dirPath, fn)))
        {
            fn = $"{baseFn}_1.zip";
            if (File.Exists(Path.Combine(dirPath, fn)))
            {
                DeleteFile(Path.Combine(dirPath, fn));
            }
        }

        var backupPath = Path.Combine(dirPath, fn);
        CreateArchive(backupPath, dirInfo);
    }

    private static void CreateArchiveOneBackup(string dirPath)
    {
        var dirInfo = new DirectoryInfo(dirPath);
        var fn = GetBaseArchiveFn(dirInfo) + ".zip";
        var backupPath = Path.Combine(dirPath, fn);
        if (File.Exists(backupPath))
        {
            DeleteFile(backupPath);
        }

        CreateArchive(backupPath, dirInfo);
    }

    private static void CreateDefaultArchiveBackup(string dirPath)
    {
        var dirInfo = new DirectoryInfo(dirPath);
        var baseFn = GetBaseArchiveFnWithTimestamp(dirInfo);

        var i = 1;

        string backupPath;

        do
        {
            var fn = i == 1 ? baseFn + ".zip" : $"{baseFn}_{i}.zip";
            backupPath = Path.Combine(dirPath, fn);

            if (!File.Exists(backupPath))
            {
                break;
            }

            i++;
        } while (true);

        CreateArchive(backupPath, dirInfo);
    }

    private static string GetBaseArchiveFnWithTimestamp(FileSystemInfo? parentDir)
    {
        return parentDir == null
            ? $"{ArchiveBaseFn}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
            : $"{ArchiveBaseFn}_{parentDir.Name}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    }

    private static string GetBaseArchiveFn(FileSystemInfo? parentDir)
    {
        return parentDir == null ? ArchiveBaseFn : $"{ArchiveBaseFn}_{parentDir.Name}";
    }

    private static void CreateArchive(string backupPath, DirectoryInfo dirInfo)
    {
        var dirName = Path.GetDirectoryName(backupPath);
        var zipPath = Path.Combine(dirName!, SavegameStore.FileType.SaveDataArchive.GetFilename());
        if (File.Exists(zipPath))
        {
            Logger.Info("SaveData archive found, copying...");
            File.Copy(zipPath, backupPath);
            return;
        }

        Logger.Info($"Creating archive at {backupPath}...");
        using var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create);
        foreach (var jsonFile in dirInfo.GetFiles("*.json"))
        {
            archive.CreateEntryFromFile(jsonFile.FullName, jsonFile.Name);
        }
    }
}