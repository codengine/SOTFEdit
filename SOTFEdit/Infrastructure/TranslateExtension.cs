using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model.Events;

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
        // Check if we're being used in a context that supports bindings
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target)
        {
            // If the target property is not a DependencyProperty, return the static string
            // This handles cases like FallbackValue, TargetNullValue, etc.
            if (target.TargetProperty is not System.Windows.DependencyProperty)
            {
                return TranslationManager.Get(_key);
            }
            
            // Create a binding to a TranslationProxy that will update when language changes
            var proxy = new TranslationProxy(_key);
            var binding = new Binding(nameof(TranslationProxy.Value))
            {
                Source = proxy,
                Mode = BindingMode.OneWay
            };
            
            return binding.ProvideValue(serviceProvider);
        }
        
        return TranslationManager.Get(_key);
    }
    
    private class TranslationProxy : INotifyPropertyChanged
    {
        private readonly string _key;

        public TranslationProxy(string key)
        {
            _key = key;
            WeakReferenceMessenger.Default.Register<LanguageChangedEvent>(this, (_, _) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                });
            });
        }

        public string Value => TranslationManager.Get(_key);

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}