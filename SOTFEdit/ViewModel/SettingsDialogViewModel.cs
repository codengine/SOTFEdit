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

    [ObservableProperty]
    private BackupModeWrapper _currentBackupMode;

    [ObservableProperty]
    private ThemeData _currentThemeAccent;

    [ObservableProperty]
    private string _selectedLanguage;
    
    [ObservableProperty]
    private bool _askForBackups;

    public SettingsDialogViewModel(ApplicationSettings applicationSettings)
    {
        _applicationSettings = applicationSettings;
        _currentThemeAccent = applicationSettings.CurrentThemeAccent;
        BackupModes = Enum.GetValues<ApplicationSettings.BackupMode>()
            .Select(backupMode => new BackupModeWrapper(backupMode))
            .ToList();
        _currentBackupMode = BackupModes.First(wrapper => wrapper.BackupMode == applicationSettings.CurrentBackupMode);
        _askForBackups = Settings.Default.AskForBackups;
        Languages = LanguageManager.GetAvailableCultures()
            .Select(culture =>
                new ComboBoxItemAndValue<string>(TranslationManager.Get("languages." + culture), culture))
            .ToList();
        _selectedLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }

    public List<ComboBoxItemAndValue<string>> Languages { get; }

    public List<BackupModeWrapper> BackupModes { get; }

    public List<ThemeData> AccentColors => _applicationSettings.AccentColors;

    [RelayCommand]
    private void Save()
    {
        _applicationSettings.CurrentThemeAccent = CurrentThemeAccent;
        _applicationSettings.CurrentBackupMode = CurrentBackupMode.BackupMode;
        Settings.Default.Language = SelectedLanguage;
        Settings.Default.AskForBackups = AskForBackups;
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

        // ReSharper disable once UnusedMember.Global
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