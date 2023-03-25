using System.Diagnostics;

namespace SOTFEdit.Model.Events;

public class RequestStartProcessEvent
{
    private RequestStartProcessEvent(ProcessStartInfo processStartInfo)
    {
        ProcessStartInfo = processStartInfo;
    }

    public ProcessStartInfo ProcessStartInfo { get; }

    public static RequestStartProcessEvent ForUrl(string url)
    {
        return new RequestStartProcessEvent(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public static RequestStartProcessEvent ForExplorer(string url)
    {
        return new RequestStartProcessEvent(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = url
        });
    }
}