using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using ControlzEx.Theming;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.Events;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class SettingsDialog
{
    private bool _saved;

    public SettingsDialog(Window owner, ApplicationSettings applicationSettings)
    {
        Owner = owner;
        DataContext = new SettingsDialogViewModel(applicationSettings);

        WeakReferenceMessenger.Default.Register<SettingsSavedEvent>(this,
            (_, _) => Application.Current.Dispatcher.Invoke(SettingsSavedEvent));

        InitializeComponent();
    }

    private void SettingsSavedEvent()
    {
        _saved = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_saved)
        {
            if (Settings.Default.Language != CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
            {
                WeakReferenceMessenger.Default.Send(new GenericMessageEvent(
                    TranslationManager.Get("windows.settings.messages.languageChanged.text"),
                    TranslationManager.Get("windows.settings.messages.languageChanged.title")));
            }

            return;
        }

        var defaultTheme = string.IsNullOrEmpty(Settings.Default.Theme) ? "Dark" : Settings.Default.Theme!;
        var defaultAccent = string.IsNullOrEmpty(Settings.Default.ThemeAccent) ? "Blue" : Settings.Default.ThemeAccent!;

        ThemeManager.Current.ChangeThemeBaseColor(Application.Current, defaultTheme);
        ThemeManager.Current.ChangeThemeColorScheme(Application.Current, defaultAccent);
    }

    private void SettingsDialog_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}