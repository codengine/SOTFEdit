using Semver;

namespace SOTFEdit.Model.Events;

public class VersionCheckResultEvent
{
    public SemVersion? LatestTagVersion { get; }
    public bool IsNewer { get; }
    public bool IsError { get; }
    public string? Link { get; init; }

    public VersionCheckResultEvent(SemVersion? latestTagVersion, bool isNewer, bool isError = false)
    {
        LatestTagVersion = latestTagVersion;
        IsNewer = isNewer;
        IsError = isError;
    }
}