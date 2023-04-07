using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SOTFEdit.View;

public partial class MarkdownViewer
{
    private readonly string _path;

    public MarkdownViewer(Window owner, string path, string title)
    {
        _path = path;
        Owner = owner;
        Loaded += OnLoaded;
        InitializeComponent();
        Title = title;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Viewer.Markdown = File.ReadAllText(_path);
    }

    private void MarkdownViewer_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}