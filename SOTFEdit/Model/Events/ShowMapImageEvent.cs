namespace SOTFEdit.Model.Events;

public class ShowMapImageEvent(string url, string title)
{
    public string Url { get; } = url;
    public string Title { get; } = title;
}