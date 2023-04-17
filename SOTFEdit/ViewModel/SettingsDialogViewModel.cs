using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ControlzEx.Theming;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;

namespace SOTFEdit.ViewModel;

public partial class SettingsDialogViewModel : ObservableObject
{
    private readonly ApplicationSettings _applicationSettings;
    private ApplicationSettings.BackupFlag _backupFlags;

    [ObservableProperty] private BackupModeWrapper _currentBackupMode;

    [ObservableProperty] private ThemeData _currentThemeAccent;

    [ObservableProperty] private string _selectedLanguage;

    public SettingsDialogViewModel(ApplicationSettings applicationSettings)
    {
        _applicationSettings = applicationSettings;
        CurrentThemeAccent = applicationSettings.CurrentThemeAccent;
        CurrentBackupMode = BackupModes.First(wrapper => wrapper.BackupMode == applicationSettings.CurrentBackupMode);
        _backupFlags = ApplicationSettings.BackupFlags;
        Languages = LanguageManager.GetAvailableCultures()
            .Select(culture =>
                new ComboBoxItemAndValue<string>(TranslationManager.Get("languages." + culture), culture))
            .ToList();
        SelectedLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }

    public List<ComboBoxItemAndValue<string>> Languages { get; }

    public List<BackupModeWrapper> BackupModes => Enum.GetValues<ApplicationSettings.BackupMode>()
        .Select(backupMode => new BackupModeWrapper(backupMode))
        .ToList();

    public List<ThemeData> AccentColors => _applicationSettings.AccentColors;

    public bool AskForBackups
    {
        get => GetBackupFlag(ApplicationSettings.BackupFlag.ASK_FOR_BACKUP);
        set => SetBackupFlag(ApplicationSettings.BackupFlag.ASK_FOR_BACKUP, value);
    }

    public bool BackupFileTypeSingle
    {
        get => GetBackupFlag(ApplicationSettings.BackupFlag.TYPE_SINGLEFILE);
        set => SetBackupFlag(ApplicationSettings.BackupFlag.TYPE_SINGLEFILE, value);
    }

    public bool BackupFileTypeArchive
    {
        get => GetBackupFlag(ApplicationSettings.BackupFlag.TYPE_ARCHIVE);
        set => SetBackupFlag(ApplicationSettings.BackupFlag.TYPE_ARCHIVE, value);
    }

    private bool GetBackupFlag(ApplicationSettings.BackupFlag flag)
    {
        return (_backupFlags & flag) != 0;
    }

    private void SetBackupFlag(ApplicationSettings.BackupFlag flag, bool value)
    {
        if (value)
        {
            _backupFlags |= flag;
        }
        else
        {
            _backupFlags &= ~flag;
        }
    }

    [RelayCommand]
    private void Save()
    {
        _applicationSettings.CurrentThemeAccent = CurrentThemeAccent;
        _applicationSettings.CurrentBackupMode = CurrentBackupMode.BackupMode;
        Settings.Default.Language = SelectedLanguage;
        ApplicationSettings.BackupFlags = _backupFlags;
        _applicationSettings.Save();
        WeakReferenceMessenger.Default.Send(new SettingsSavedEvent());
    }

    partial void OnCurrentThemeAccentChanged(ThemeData value)
    {
        ThemeManager.Current.ChangeThemeColorScheme(Application.Current, value.Name);
    }

    public class BackupModeWrapper
    {
        public BackupModeWrapper(ApplicationSettings.BackupMode backupMode)
        {
            BackupMode = backupMode;
        }

        public ApplicationSettings.BackupMode BackupMode { get; }

        public string Name => TranslationManager.Get("backup.mode." + BackupMode);

        private bool Equals(BackupModeWrapper other)
        {
            return BackupMode == other.BackupMode;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((BackupModeWrapper)obj);
        }

        public override int GetHashCode()
        {
            return (int)BackupMode;
        }
    }
}