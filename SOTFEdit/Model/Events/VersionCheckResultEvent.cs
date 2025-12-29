using Semver;

namespace SOTFEdit.Model.Events;

public class VersionCheckResultEvent(
    SemVersion? latestTagVersion, bool isNewer, bool invokedManually,
    string? changelog = null,
    bool isError = false)
{
    public SemVersion? LatestTagVersion { get; } = latestTagVersion;
    public bool IsNewer { get; } = isNewer;
    public bool InvokedManually { get; } = invokedManually;
    public string? Changelog { get; } = changelog;
    public bool IsError { get; } = isError;
    public string? Link { get; init; }
}