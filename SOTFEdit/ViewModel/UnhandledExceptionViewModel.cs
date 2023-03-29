using System;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SOTFEdit.ViewModel;

// ReSharper disable once ClassNeverInstantiated.Global
public partial class UnhandledExceptionViewModel : ObservableObject
{
    public UnhandledExceptionViewModel(Exception exception)
    {
        Exception = exception;
    }

    public Exception Exception { get; }
    public string? ExceptionType => Exception.GetType().FullName;

    public static string ApplicationVersion
    {
        get
        {
            App.GetAssemblyVersion(out _, out var assemblyVersion);
            return assemblyVersion.ToString();
        }
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        var text = new StringBuilder();
        App.GetAssemblyVersion(out _, out var assemblyVersion);
        text.AppendLine($"Version: {assemblyVersion}");
        text.AppendLine($"Message: {Exception.Message}");
        text.AppendLine($"Exception Type: {ExceptionType}");
        text.AppendLine($"Callstack: {Exception.StackTrace}");
        text.AppendLine($"Inner Exception: {Exception.InnerException}");
        Clipboard.SetText(text.ToString());
    }

    [RelayCommand]
    private static void ExitApplication()
    {
        Environment.Exit(-1);
    }
}