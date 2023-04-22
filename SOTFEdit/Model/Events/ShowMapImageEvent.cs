namespace SOTFEdit.Model.Events;

public class ShowMapImageEvent
{
    public ShowMapImageEvent(string url, string title)
    {
        Url = url;
        Title = title;
    }

    public string Url { get; }
    public string Title { get; }
}