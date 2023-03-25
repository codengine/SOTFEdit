using System;
using Newtonsoft.Json.Linq;

namespace SOTFEdit;

public static class JsonModifier
{
    public static bool CompareAndModify(JToken? token, Func<float, bool> comparator, float newValue)
    {
        if (token == null)
        {
            return false;
        }

        var oldValue = token.Value<float>();
        if (!comparator.Invoke(oldValue))
        {
            return false;
        }

        token.Replace(newValue);
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