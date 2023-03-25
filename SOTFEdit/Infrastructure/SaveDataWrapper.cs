using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SOTFEdit.Infrastructure;

public class SaveDataWrapper
{
    private readonly Dictionary<string, JToken?> _jsonStringNodes = new();
    private readonly HashSet<string> _modified = new();
    private readonly JToken _parent;

    public SaveDataWrapper(JToken parent)
    {
        _parent = parent;
    }

    public JToken? GetJsonBasedToken(string selector)
    {
        if (_jsonStringNodes.ContainsKey(selector))
        {
            return _jsonStringNodes[selector];
        }

        var token = _parent.SelectToken(selector);
        if (token is not { Type: JTokenType.String })
        {
            return token;
        }

        token = Deserialize(token);
        _jsonStringNodes.Add(selector, token);
        return token;
    }

    public bool SerializeAllModified()
    {
        var hasChanges = false;

        if (_modified.Count == 0)
        {
            return hasChanges;
        }

        foreach (var key in _modified)
        {
            if (!_jsonStringNodes.TryGetValue(key, out var token))
            {
                continue;
            }

            hasChanges = true;
            _parent.SelectToken(key)?.Replace(JsonConverter.Serialize(token));
        }

        return hasChanges;
    }

    public void MarkAsModified(string selector)
    {
        _modified.Add(selector);
    }

    private static JToken? Deserialize(JToken token)
    {
        return token.ToString() is { } json ? JsonConverter.DeserializeRaw(json) : null;
    }
}