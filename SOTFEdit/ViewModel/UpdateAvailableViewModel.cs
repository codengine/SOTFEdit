using Semver;

namespace SOTFEdit.ViewModel;

public class UpdateAvailableViewModel
{
    public UpdateAvailableViewModel(string? changelog, SemVersion? version)
    {
        Changelog = changelog;
        Version = version;
    }

    public string? Changelog { get; }
    public SemVersion? Version { get; }
}