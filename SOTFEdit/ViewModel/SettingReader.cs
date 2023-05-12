using Newtonsoft.Json.Linq;
using static SOTFEdit.Model.Constants.SettingValueKeys;

namespace SOTFEdit.ViewModel;

public static class SettingReader
{
    public static float? ReadFloat(JToken? token)
    {
        return token?[FloatValue]?.Value<float>();
    }

    public static int? ReadInt(JToken? token)
    {
        return token?[IntValue]?.Value<int>();
    }

    public static bool ReadString(JToken? token, out string? value)
    {
        var valueToken = token?[StringValue];
        if (valueToken == null)
        {
            value = null;
            return false;
        }

        value = valueToken.Value<string>();
        return true;
    }

    public static bool? ReadBool(JToken? token)
    {
        return token?[BoolValue]?.Value<bool>();
    }
}