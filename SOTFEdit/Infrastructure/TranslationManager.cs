using System.Collections.Generic;
using JetBrains.Annotations;
using libc.translation;

namespace SOTFEdit.Infrastructure;

public static class TranslationManager
{
    private static readonly ILocalizer Localizer = LanguageManager.BuildLocalizer();

    [ContractAnnotation("fallback:null => null")]
    public static string Get(string key, string? fallback = null, bool fallbackIsTranslationKey = true)
    {
        var translated = Localizer.Get(key);
        if (translated != key)
        {
            return translated;
        }

        if (fallback != null)
        {
            return fallbackIsTranslationKey ? Localizer.Get(fallback) : fallback;
        }

        return translated;
    }

    public static string GetFormatted(string key, params object[]? args)
    {
        if (args == null || args.Length == 0)
        {
            return Get(key);
        }

        return Localizer.GetFormat(key, args);
    }

    public static IDictionary<string, string> GetAll(string culture)
    {
        return Localizer.GetAll(culture);
    }
}