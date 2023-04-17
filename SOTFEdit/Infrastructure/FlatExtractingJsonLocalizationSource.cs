using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using libc.translation;

namespace SOTFEdit.Infrastructure;

public class FlatExtractingJsonLocalizationSource: ILocalizationSource
{
    private readonly PropertyCaseSensitivity _caseSensitivity;
    private readonly string _filenamePattern;

    private readonly ConcurrentDictionary<string, IDictionary<string, string>> _strings = new();
    private readonly string _path;

    public FlatExtractingJsonLocalizationSource(string path, PropertyCaseSensitivity caseSensitivity, string filenamePattern = "{0}.json")
    {
        _caseSensitivity = caseSensitivity;
        _filenamePattern = filenamePattern;
        _path = path;
    }

    private IDictionary<string,string> LoadStrings(string jsonFile)
    {
        var document = JsonDocument.Parse(new FileInfo(jsonFile).OpenRead());
        return document.RootElement
            .EnumerateObject()
            .Aggregate(new Dictionary<string, string>(), (acc, cur) =>
            {
                if (cur.Value.ValueKind != JsonValueKind.Object && cur.Value.ValueKind != JsonValueKind.Array)
                {
                    acc[GetKey(cur.Name)] = cur.Value.ToString();
                }
                else
                {
                    var flatChild = FlattenJson(cur.Value);
                    foreach (var item in flatChild)
                    {
                        acc[GetKey($"{cur.Name}.{item.Key}")] = item.Value;
                    }
                }
                return acc;
            });
    }

    private string GetKey(string key)
    {
        return _caseSensitivity == PropertyCaseSensitivity.CaseInsensitive ? key.ToLower() : key;
    }

    private static Dictionary<string, string> FlattenJson(JsonElement element, string prefix = "")
    {
        var dictionary = new Dictionary<string, string>();

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var nestedPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    var childDictionary = FlattenJson(property.Value, nestedPrefix);
                    foreach (var childProperty in childDictionary)
                    {
                        dictionary[childProperty.Key] = childProperty.Value;
                    }
                }
                break;
            case JsonValueKind.Array:
                for (var i = 0; i < element.GetArrayLength(); i++)
                {
                    var nestedPrefix = string.IsNullOrEmpty(prefix) ? $"{i}" : $"{prefix}.{i}";
                    var childDictionary = FlattenJson(element[i], nestedPrefix);
                    foreach (var childProperty in childDictionary)
                    {
                        dictionary[childProperty.Key] = childProperty.Value;
                    }
                }
                break;
            default:
                dictionary[prefix] = element.ToString();
                break;
        }

        return dictionary;
    }

    public string? Get(string culture, string key)
    {
        return GetStrings(culture).TryGetValue(GetKey(key), out var value) ? value : null;
    }

    private IDictionary<string, string> GetStrings(string culture)
    {
        return _strings.GetOrAdd(culture, ReadFile);
    }

    private IDictionary<string,string> ReadFile(string key)
    {
        var jsonFile = Path.Combine(_path, string.Format(_filenamePattern, key));
        return !File.Exists(jsonFile) ? new Dictionary<string, string>() : LoadStrings(jsonFile);
    }

    public IDictionary<string, string> GetAll(string culture)
    {
        return _strings.GetValueOrDefault(culture) ?? new ConcurrentDictionary<string, string>();
    }
}