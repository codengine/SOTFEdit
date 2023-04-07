using Semver;

namespace SOTFEdit.Model.Events;

public class VersionCheckResultEvent
{
    public VersionCheckResultEvent(SemVersion? latestTagVersion, bool isNewer, bool invokedManually,
        string? changelog = null,
        bool isError = false)
    {
        LatestTagVersion = latestTagVersion;
        IsNewer = isNewer;
        InvokedManually = invokedManually;
        Changelog = changelog;
        IsError = isError;
    }

    public SemVersion? LatestTagVersion { get; }
    public bool IsNewer { get; }
    public bool InvokedManually { get; }
    public string? Changelog { get; }
    public bool IsError { get; }
    public string? Link { get; init; }
}