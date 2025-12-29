using System;
using System.Windows.Markup;

namespace SOTFEdit.Infrastructure;

public class TranslateExtension(string key) : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return TranslationManager.Get(key);
    }
}