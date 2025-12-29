using Semver;

namespace SOTFEdit.ViewModel;

public class UpdateAvailableViewModel(string? changelog, SemVersion? version)
{
    public string? Changelog { get; } = changelog;
    public SemVersion? Version { get; } = version;
}