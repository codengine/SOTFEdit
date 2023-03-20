using System.Diagnostics;

namespace SOTFEdit.Model.Events;

public class RequestStartProcessEvent
{
    public ProcessStartInfo ProcessStartInfo { get; }

    public RequestStartProcessEvent(ProcessStartInfo processStartInfo)
    {
        ProcessStartInfo = processStartInfo;
    }

    public static RequestStartProcessEvent ForUrl(string url)
    {
        return new RequestStartProcessEvent(new ProcessStartInfo()
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public static RequestStartProcessEvent ForExplorer(string url)
    {
        return new RequestStartProcessEvent(new ProcessStartInfo()
        {
            FileName = "explorer.exe",
            Arguments = url
        });
    }
}