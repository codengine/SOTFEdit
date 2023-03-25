using System.Diagnostics.CodeAnalysis;

namespace SOTFEdit.Model.SaveData;

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public abstract record SotfBaseModel(string Version = "0.0.0");