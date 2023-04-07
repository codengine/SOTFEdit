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

    public static void BackupSingleFile(string fullPath, ApplicationSettings.BackupMode backupMode)
    {
        switch (backupMode)
        {
            case ApplicationSettings.BackupMode.None:
                return;
            case ApplicationSettings.BackupMode.Default:
                CreateDefaultSingleFileBackup(fullPath);
                break;
            case ApplicationSettings.BackupMode.One:
                CreateSingleFileOneBackup(fullPath);
                break;
            case ApplicationSettings.BackupMode.InitialAndOne:
                CreateSingleFileInitialAndOneBackup(fullPath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(backupMode), backupMode, null);
        }
    }

    private static void CreateSingleFileInitialAndOneBackup(string fullPath)
    {
        var backupPath = fullPath + ".bak";
        if (!File.Exists(backupPath))
        {
            File.Copy(fullPath, backupPath);
            return;
        }

        backupPath = fullPath + ".bak1";
        if (File.Exists(backupPath))
        {
            DeleteFile(backupPath);
        }

        File.Copy(fullPath, backupPath);
    }

    private static void CreateSingleFileOneBackup(string fullPath)
    {
        var backupPath = fullPath + ".bak";
        if (!File.Exists(backupPath))
        {
            File.Copy(fullPath, backupPath);
        }
    }

    private static void CreateDefaultSingleFileBackup(string fullPath)
    {
        var backupPath = fullPath + ".bak";
        if (File.Exists(backupPath))
        {
            var i = 1;

            do
            {
                backupPath = fullPath + $".bak{i++}";
            } while (File.Exists(backupPath));
        }

        File.Copy(fullPath, backupPath);
    }

    public static int DeleteAllBackups(string path, ApplicationSettings.BackupFlag backupFlags)
    {
        if (!Directory.Exists(path))
        {
            return 0;
        }

        if (IsSingleFileBackup(backupFlags))
        {
            return DeleteAllSingleFileBackups(path);
        }

        if (IsArchiveBackup(backupFlags))
        {
            return DeleteAllArchiveBackups(path);
        }

        return 0;
    }

    private static int DeleteAllArchiveBackups(string path)
    {
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

    private static int DeleteAllSingleFileBackups(string path)
    {
        var countDeleted = 0;

        foreach (var fileType in Enum.GetValues<SavegameStore.FileType>())
        {
            var singleBackupFiles = GetSingleBackupFiles(path, fileType.GetFilename());
            foreach (var backupFile in singleBackupFiles)
            {
                Logger.Info($"Deleting backup {backupFile.FullName}...");
                DeleteFile(backupFile.FullName);
                countDeleted++;
            }
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

    private static IEnumerable<FileInfo> GetSingleBackupFiles(string path, string prefix = "*")
    {
        return new DirectoryInfo(path).GetFiles($"{prefix}.bak*", SearchOption.TopDirectoryOnly);
    }

    private static FileInfo? GetBackupArchive(string path, bool newestFile)
    {
        return (newestFile
                ? GetBackupArchiveFiles(path).OrderByDescending(fileInfo => fileInfo.CreationTimeUtc)
                : GetBackupArchiveFiles(path).OrderBy(fileInfo => fileInfo.CreationTimeUtc))
            .FirstOrDefault();
    }

    private static FileInfo? GetSingleBackupFile(string path, SavegameStore.FileType fileType, bool newestFile)
    {
        return (newestFile
                ? GetSingleBackupFiles(path, fileType.GetFilename())
                    .OrderByDescending(fileInfo => fileInfo.CreationTimeUtc)
                : GetSingleBackupFiles(path, fileType.GetFilename()).OrderBy(fileInfo => fileInfo.CreationTimeUtc))
            .FirstOrDefault();
    }

    public static int RestoreBackups(string path, bool restoreFromNewest, ApplicationSettings.BackupFlag backupFlags)
    {
        if (IsSingleFileBackup(backupFlags))
        {
            return RestoreBackupsFromSingleFiles(path, restoreFromNewest);
        }

        if (IsArchiveBackup(backupFlags))
        {
            return RestoreBackupsFromArchive(path, restoreFromNewest);
        }

        return 0;
    }

    private static bool IsArchiveBackup(ApplicationSettings.BackupFlag backupFlags)
    {
        return (backupFlags & ApplicationSettings.BackupFlag.TYPE_ARCHIVE) != 0;
    }

    private static bool IsSingleFileBackup(ApplicationSettings.BackupFlag backupFlags)
    {
        return (backupFlags & ApplicationSettings.BackupFlag.TYPE_SINGLEFILE) != 0;
    }

    private static int RestoreBackupsFromArchive(string path, bool restoreFromNewest)
    {
        var archive = GetBackupArchive(path, restoreFromNewest);
        if (archive == null)
        {
            Logger.Info("No backup found");
            return 0;
        }

        Logger.Info($"Extracting backups from {archive.FullName} to {path}");
        var zipArchive = ZipFile.OpenRead(archive.FullName);
        zipArchive.ExtractToDirectory(path, true);

        return 1;
    }

    private static int RestoreBackupsFromSingleFiles(string path, bool restoreFromNewest)
    {
        var restored = 0;
        foreach (var fileType in Enum.GetValues<SavegameStore.FileType>())
        {
            if (GetSingleBackupFile(path, fileType, restoreFromNewest) is not { } fileInfo)
            {
                continue;
            }

            RestoreBackupFromSingleFile(path, fileType, fileInfo);
            restored++;
        }

        return restored;
    }

    private static void RestoreBackupFromSingleFile(string path, SavegameStore.FileType fileType,
        FileSystemInfo backupFile)
    {
        var originalFilePath = Path.Combine(path, fileType.GetFilename());
        if (File.Exists(originalFilePath))
        {
            Logger.Info($"Deleting original file {originalFilePath}");
            FileSystem.DeleteFile(originalFilePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }

        Logger.Info($"Copying backup {backupFile.FullName} to {originalFilePath}");
        File.Copy(backupFile.FullName, originalFilePath);
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
        Logger.Info($"Creating archive at {backupPath}...");
        using var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create);
        foreach (var jsonFile in dirInfo.GetFiles("*.json"))
            archive.CreateEntryFromFile(jsonFile.FullName, jsonFile.Name);
    }
}