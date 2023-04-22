namespace SOTFEdit.ViewModel;

public class ShowImageWindowViewModel
{
    public ShowImageWindowViewModel(string url, string title)
    {
        Url = url;
        Title = title;
    }

    public string Url { get; }
    public string Title { get; }
}