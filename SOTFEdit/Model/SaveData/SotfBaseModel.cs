using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace SOTFEdit.Model.SaveData;

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public abstract record SotfBaseModel([property: JsonProperty(Order = -99999)]
    string Version = "0.0.0");