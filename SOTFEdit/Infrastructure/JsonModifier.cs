using System;
using Newtonsoft.Json.Linq;

namespace SOTFEdit.Infrastructure;

public static class JsonModifier
{
    public static bool CompareAndModify(JToken? token, string key, Func<float, bool> comparator, float newValue)
    {
        if (token == null)
        {
            return false;
        }

        var oldValue = token[key]?.Value<float>();
        if (oldValue is { } theOldValue && !comparator.Invoke(theOldValue))
        {
            return false;
        }

        token[key] = newValue;
        return true;
    }

    public static bool CompareAndModify(JToken? token, int expectedValue)
    {
        if (token == null)
        {
            return false;
        }

        var oldValue = token.Value<int>();
        if (oldValue == expectedValue)
        {
            return false;
        }

        token.Replace(expectedValue);
        return true;
    }
}