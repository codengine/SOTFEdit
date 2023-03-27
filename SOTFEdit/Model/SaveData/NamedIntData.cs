using System.Diagnostics.CodeAnalysis;

namespace SOTFEdit.Model.SaveData;

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record NamedIntData(string SaveObjectNameId, int SaveValue);