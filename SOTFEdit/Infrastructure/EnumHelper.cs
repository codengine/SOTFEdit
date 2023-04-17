using System;
using System.Collections.Generic;
using System.Linq;

namespace SOTFEdit.Infrastructure;

public static class EnumHelper
{
    public static IEnumerable<object> GetAllEnumValues(Type type)
    {
        return Enum.GetValues(type).Cast<object>().ToList();
    }
}