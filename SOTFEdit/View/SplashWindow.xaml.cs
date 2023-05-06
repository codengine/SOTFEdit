using System.Threading.Tasks;
using System.Windows;

namespace SOTFEdit.View;

public partial class SplashWindow
{
    private MainWindow? _mainWindow;

    public SplashWindow()
    {
        App.GetAssemblyVersion(out var assemblyName, out var assemblyVersion);
        AssemblyVersion = $"v{assemblyVersion}";
        DataContext = this;
        InitializeComponent();
        Title = assemblyName;
        Loaded += SplashWindow_Loaded;
    }

    public string AssemblyVersion { get; }

    private async void SplashWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Create a TaskCompletionSource to signal when the MainWindow is shown
        var mainWindowShown = new TaskCompletionSource<bool>();

        var delay = 500;

        if (Settings.Default.FirstRun)
        {
            delay = 2000;
            Settings.Default.FirstRun = false;
            Settings.Default.Save();
        }

        // Create a task to load the MainWindow
        var loadMainWindowTask = Task.Run(() =>
        {
            // Invoke the Show method on the UI thread
            Dispatcher.Invoke(() =>
            {
                _mainWindow = new MainWindow();
                _mainWindow.Show();
                // Signal that the MainWindow is shown
                mainWindowShown.SetResult(true);
            });
        });

        // Create a delay task of 2 seconds (2000 milliseconds)
        var delayTask = Task.Delay(delay);

        // Wait for either the loadMainWindowTask or the delayTask to complete, whichever happens first
        await Task.WhenAll(loadMainWindowTask, delayTask);

        // Wait for the MainWindow to be shown
        await mainWindowShown.Task;

        // Close the splash window
        Close();
    }
}