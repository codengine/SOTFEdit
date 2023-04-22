using System;
using System.Collections.Generic;
using System.IO;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Map.Static;

public static class RawPoiGroupLoader
{
    public static Dictionary<string, RawPoiGroup> Load()
    {
        return JsonConverter.DeserializeFromFile<Dictionary<string, RawPoiGroup>>(
                   Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "pois.json")) ??
               new Dictionary<string, RawPoiGroup>();
    }
}