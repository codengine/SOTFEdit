using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Savegame;

public class SaveDataWrapper
{
    private readonly Dictionary<string, JToken?> _jsonStringNodes = new();
    private readonly HashSet<string> _modified = new();
    private readonly ReaderWriterLockSlim _readerWriterLockSlim = new();

    public SaveDataWrapper(JToken parent)
    {
        Parent = parent;
    }

    public JToken Parent { get; }

    public JToken? GetJsonBasedToken(string selector)
    {
        _readerWriterLockSlim.EnterUpgradeableReadLock();
        try
        {
            if (_jsonStringNodes.TryGetValue(selector, out var jsonBasedToken))
            {
                return jsonBasedToken;
            }

            var token = Parent.SelectToken(selector);
            if (token is not { Type: JTokenType.String })
            {
                return token;
            }

            token = Deserialize(token);
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                if (_jsonStringNodes.TryGetValue(selector, out var basedToken))
                {
                    return basedToken;
                }

                _jsonStringNodes.Add(selector, token);
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }

            return token;
        }
        finally
        {
            _readerWriterLockSlim.ExitUpgradeableReadLock();
        }
    }

    public bool HasModified()
    {
        return _modified.Count > 0;
    }

    public void SerializeAllModified()
    {
        foreach (var key in _modified)
        {
            if (!_jsonStringNodes.TryGetValue(key, out var token))
            {
                continue;
            }

            Parent.SelectToken(key)?.Replace(JsonConverter.Serialize(token));
        }

        _modified.Clear();
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