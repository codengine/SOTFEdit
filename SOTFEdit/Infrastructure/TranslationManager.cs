using System.Resources;

namespace SOTFEdit.Infrastructure;

public static class TranslationManager
{
    private static readonly ResourceManager ResourceManager = new(typeof(Resources));

    public static string GetString(string prefix, string? suffix, string defaultValue = "")
    {
        if (suffix == null)
        {
            return defaultValue;
        }

        return ResourceManager.GetString(prefix + suffix) ?? suffix;
    }
}