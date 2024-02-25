using System;
using Newtonsoft.Json.Linq;
using static SOTFEdit.Model.Constants.SettingValueKeys;

namespace SOTFEdit.ViewModel;

public static class SettingWriter
{
    public static bool WriteFloat(JToken? target, float newValue)
    {
        if (target == null ||
            (SettingReader.ReadFloat(target) is { } oldValue && Math.Abs(oldValue - newValue) < 0.001))
        {
            return false;
        }

        target[FloatValue] = newValue;
        return true;
    }

    public static bool WriteInt(JToken? target, int newValue)
    {
        if (target == null || (SettingReader.ReadInt(target) is { } oldValue && oldValue == newValue))
        {
            return false;
        }

        target[IntValue] = newValue;
        return true;
    }

    public static bool RemoveFloat(JToken? target)
    {
        if (target is not JObject obj)
        {
            return false;
        }

        obj.Remove(FloatValue);

        return true;
    }
}