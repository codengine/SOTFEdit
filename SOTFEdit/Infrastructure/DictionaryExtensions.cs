using System.Collections.Generic;

namespace SOTFEdit.Infrastructure;

public static class DictionaryExtensions
{
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        where TValue : new()
    {
        if (dict.TryGetValue(key, out var val))
        {
            return val;
        }

        val = new TValue();
        dict.Add(key, val);

        return val;
    }
}