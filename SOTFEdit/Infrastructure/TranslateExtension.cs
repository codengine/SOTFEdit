using System;
using System.Windows.Markup;

namespace SOTFEdit.Infrastructure;

public class TranslateExtension : MarkupExtension
{
    private readonly string _key;

    public TranslateExtension(string key)
    {
        _key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return TranslationManager.Get(_key);
    }
}