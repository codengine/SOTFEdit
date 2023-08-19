using System.Collections.Generic;
using System.Linq;
using ControlzEx.Theming;
using SOTFEdit.Infrastructure;

namespace SOTFEdit;

public class ApplicationSettings
{
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

    public void Save()
    {
        Settings.Default.Save();
    }
}