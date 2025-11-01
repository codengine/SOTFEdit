using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.ViewModel;

public partial class TranslationViewModel : ObservableObject
{
    private readonly ICloseable _owner;
    private readonly ObservableCollection<TranslationEntry> _translationEntries;

    [ObservableProperty]
    private string _filterText = "";

    [ObservableProperty]
    private bool _onlyMissing;

    public TranslationViewModel(ICloseable owner)
    {
        _owner = owner;
        var deEntries = TranslationManager.GetAll("de");
        var plEntries = TranslationManager.GetAll("pl");

        var naturalStringComparer = new NaturalStringComparer();

        var enEntries = TranslationManager.GetAll("en").Select(
                kvp =>
                {
                    deEntries.TryGetValue(kvp.Key, out var deValue);
                    plEntries.TryGetValue(kvp.Key, out var plValue);

                    return new TranslationEntry(kvp.Key, kvp.Value, deValue, plValue);
                })
            .OrderBy(entry => entry.Key, naturalStringComparer)
            .ToList();
        _translationEntries = new ObservableCollection<TranslationEntry>(enEntries);
        TranslationEntries = CollectionViewSource.GetDefaultView(_translationEntries);
        TranslationEntries.Filter = OnFilterTranslationEntries;
    }

    public ICollectionView TranslationEntries { get; }

    [RelayCommand]
    private void DoFilter()
    {
        TranslationEntries.Refresh();
    }

    [RelayCommand]
    private void Save()
    {
        var enRoot = new Dictionary<string, object>();
        var deRoot = new Dictionary<string, object>();
        var plRoot = new Dictionary<string, object>();

        foreach (var translationEntry in _translationEntries)
        {
            var keys = translationEntry.Key.Split('.');
            if (!string.IsNullOrWhiteSpace(translationEntry.En))
            {
                AddToDictionary(enRoot, keys, translationEntry.En);
            }

            if (!string.IsNullOrWhiteSpace(translationEntry.De))
            {
                AddToDictionary(deRoot, keys, translationEntry.De);
            }

            if (!string.IsNullOrWhiteSpace(translationEntry.Pl))
            {
                AddToDictionary(plRoot, keys, translationEntry.Pl);
            }
        }

        var enJson = JsonConvert.SerializeObject(enRoot, Formatting.Indented);
        var deJson = JsonConvert.SerializeObject(deRoot, Formatting.Indented);
        var plJson = JsonConvert.SerializeObject(plRoot, Formatting.Indented);

        File.WriteAllText(Path.Combine(LanguageManager.LangPath, "en.json"), enJson, Encoding.UTF8);
        File.WriteAllText(Path.Combine(LanguageManager.LangPath, "de.json"), deJson, Encoding.UTF8);
        File.WriteAllText(Path.Combine(LanguageManager.LangPath, "pl.json"), plJson, Encoding.UTF8);

        _owner.Close();
    }

    private static void AddToDictionary(Dictionary<string, object> dict, IReadOnlyList<string> keys, string value)
    {
        for (var i = 0; i < keys.Count; i++)
        {
            if (i == keys.Count - 1) // Last key, add the value
            {
                dict[keys[i]] = value;
            }
            else
            {
                if (!dict.ContainsKey(keys[i]) || dict[keys[i]] is not Dictionary<string, object>)
                {
                    dict[keys[i]] = new Dictionary<string, object>();
                }
                dict = dict[keys[i]] as Dictionary<string, object> ?? throw new InvalidOperationException("Dictionary structure corrupted");
            }
        }
    }

    private bool OnFilterTranslationEntries(object obj)
    {
        var filterText = FilterText;
        if (!OnlyMissing && string.IsNullOrWhiteSpace(filterText))
        {
            return true;
        }

        if (obj is not TranslationEntry entry)
        {
            return false;
        }

        if (OnlyMissing && !string.IsNullOrWhiteSpace(entry.De) && !string.IsNullOrWhiteSpace(entry.Pl))
        {
            return false;
        }

        var comparisonResult = string.IsNullOrWhiteSpace(filterText)
                               || entry.Key.Contains(filterText, StringComparison.OrdinalIgnoreCase)
                               || entry.En.Contains(filterText, StringComparison.OrdinalIgnoreCase)
                               || (entry.De?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false)
                               || (entry.Pl?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false);

        return comparisonResult;
    }
}

public partial class TranslationEntry : ObservableObject
{
    [ObservableProperty]
    private string? _de;

    [ObservableProperty]
    private string _en;

    [ObservableProperty]
    private string _key;

    [ObservableProperty]
    private string? _pl;

    public TranslationEntry(string key, string en, string? de = null, string? pl = null)
    {
        _key = key;
        _en = en;
        _de = de;
        _pl = pl;
    }
}