using System.Globalization;
using System.Linq;
using System.Text;

namespace SOTFEdit.Infrastructure;

public static class TranslationHelper
{
    public static string Normalize(string str)
    {
        return new string(str
            .Normalize(NormalizationForm.FormD).ToCharArray()
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray()
        );
    }
}