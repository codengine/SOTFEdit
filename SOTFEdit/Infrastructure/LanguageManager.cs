using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using libc.translation;

namespace SOTFEdit.Infrastructure;

public static class LanguageManager
{
    public static string LangPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "lang");

    public static IEnumerable<string> GetAvailableCultures()
    {
        return new DirectoryInfo(LangPath).GetFiles("*.json")
            .Select(fileInfo => Path.GetFileNameWithoutExtension(fileInfo.FullName))
            .ToList();
    }

    public static ILocalizer BuildLocalizer()
    {
        ILocalizationSource source =
            new FlatExtractingJsonLocalizationSource(LangPath, PropertyCaseSensitivity.CaseSensitive);
        return new Localizer(source);
    }
}