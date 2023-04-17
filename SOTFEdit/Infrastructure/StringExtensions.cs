using System;

namespace SOTFEdit.Infrastructure;

public static class StringExtensions
{
    public static string FirstCharToUpper(this string input)
    {
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException(TranslationManager.GetFormatted("errors.canNotBeEmpty", nameof(input))),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
    }
}