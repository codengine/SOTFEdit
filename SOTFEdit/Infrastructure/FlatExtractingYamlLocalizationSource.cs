using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using libc.translation;
using YamlDotNet.Serialization;

namespace SOTFEdit.Infrastructure;

/// <summary>
/// YAML-based localization source that flattens nested YAML structures into dot-notation keys.
/// Compatible with the existing FlatExtractingJsonLocalizationSource behavior.
/// </summary>
public class FlatExtractingYamlLocalizationSource : ILocalizationSource
{
    private readonly string _basePath;
    private readonly PropertyCaseSensitivity _caseSensitivity;
    private readonly IDeserializer _yamlDeserializer;
    private readonly Dictionary<string, Dictionary<string, string>> _translationCache;

    public FlatExtractingYamlLocalizationSource(string basePath, PropertyCaseSensitivity caseSensitivity)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        _caseSensitivity = caseSensitivity;
        _translationCache = new Dictionary<string, Dictionary<string, string>>();
        
        _yamlDeserializer = new DeserializerBuilder()
            .Build();
    }

    public Dictionary<string, string> GetTranslations(CultureInfo culture)
    {
        // Check cache first
        var cultureName = culture.Name;
        if (_translationCache.TryGetValue(cultureName, out var cachedTranslations))
        {
            return cachedTranslations;
        }

        var yamlFilePath = Path.Combine(_basePath, $"{cultureName}.yaml");
        
        if (!File.Exists(yamlFilePath))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            var yamlContent = File.ReadAllText(yamlFilePath);
            var data = _yamlDeserializer.Deserialize<Dictionary<object, object>>(yamlContent);
            
            if (data == null)
            {
                return new Dictionary<string, string>();
            }

            var flatTranslations = new Dictionary<string, string>(
                _caseSensitivity == PropertyCaseSensitivity.CaseInsensitive 
                    ? StringComparer.OrdinalIgnoreCase 
                    : StringComparer.Ordinal
            );

            FlattenDictionary(data, string.Empty, flatTranslations);
            
            // Cache the translations
            _translationCache[cultureName] = flatTranslations;
            
            return flatTranslations;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load YAML translations from '{yamlFilePath}'", ex);
        }
    }

    private void FlattenDictionary(Dictionary<object, object> source, string prefix, Dictionary<string, string> target)
    {
        foreach (var kvp in source)
        {
            var key = kvp.Key?.ToString();
            if (key == null) continue;
            
            var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";

            if (kvp.Value is Dictionary<object, object> nestedDict)
            {
                FlattenDictionary(nestedDict, fullKey, target);
            }
            else if (kvp.Value != null)
            {
                var value = kvp.Value.ToString();
                if (value != null)
                {
                    target[fullKey] = value;
                }
            }
        }
    }

    public IEnumerable<CultureInfo> GetAvailableCultures()
    {
        if (!Directory.Exists(_basePath))
        {
            return Enumerable.Empty<CultureInfo>();
        }

        return Directory.GetFiles(_basePath, "*.yaml")
            .Select(file => Path.GetFileNameWithoutExtension(file))
            .Where(cultureName => !string.IsNullOrEmpty(cultureName))
            .Select(cultureName =>
            {
                try
                {
                    return new CultureInfo(cultureName);
                }
                catch
                {
                    return null;
                }
            })
            .Where(culture => culture != null)
            .Cast<CultureInfo>()
            .ToList();
    }

    public string Get(string culture, string key)
    {
        try
        {
            var cultureInfo = new CultureInfo(culture);
            var translations = GetTranslations(cultureInfo);
            return translations.TryGetValue(key, out var value) ? value : key;
        }
        catch
        {
            return key;
        }
    }

    public IDictionary<string, string> GetAll(string culture)
    {
        try
        {
            var cultureInfo = new CultureInfo(culture);
            return GetTranslations(cultureInfo);
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
}
