using System;
using System.Collections.Generic;
using System.Linq;
using ControlzEx.Theming;
using SOTFEdit.Infrastructure;

namespace SOTFEdit;

public class ApplicationSettings
{
    [Flags]
    public enum BackupFlag
    {
        TYPE_SINGLEFILE = 1,
        TYPE_ARCHIVE = 2,
        ASK_FOR_BACKUP = 4
    }

    public enum BackupMode
    {
        None,
        Default,
        One,
        InitialAndOne
    }

    private ThemeData _accent;

    public ApplicationSettings()
    {
        AccentColors = ThemeManager.Current.Themes
            .GroupBy(x => x.ColorScheme)
            .OrderBy(a => a.Key)
            .Select(a => new ThemeData(a.Key, a.First().ShowcaseBrush))
            .ToList();

        var appTheme = ThemeManager.Current.DetectTheme();

        _accent = AccentColors.FirstOrDefault(accent =>
                      accent.Name == Settings.Default.ThemeAccent || accent.Name == appTheme?.ColorScheme) ??
                  AccentColors.First();
    }

    public static BackupFlag BackupFlags
    {
        get => Settings.Default.BackupFlags;
        set => Settings.Default.BackupFlags = value;
    }

    public List<ThemeData> AccentColors { get; }

    public ThemeData CurrentThemeAccent
    {
        get => _accent;
        set
        {
            Settings.Default.ThemeAccent = value.Name;
            _accent = value;
        }
    }

    public BackupMode CurrentBackupMode
    {
        get => Settings.Default.BackupMode;
        set => Settings.Default.BackupMode = value;
    }

    public bool HasBackupFlag(BackupFlag flag)
    {
        return (Settings.Default.BackupFlags & flag) != 0;
    }

    public void Save()
    {
        Settings.Default.Save();
    }
}