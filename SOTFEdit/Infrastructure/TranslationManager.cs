using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using libc.translation;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Infrastructure;

public static class TranslationManager
{
    private static readonly ILocalizer Localizer = LanguageManager.BuildLocalizer();

    public static event EventHandler? LanguageChanged;

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

    public static void ChangeCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        
        LanguageChanged?.Invoke(null, EventArgs.Empty);
        WeakReferenceMessenger.Default.Send(new LanguageChangedEvent());
    }
}