using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Xaml.Behaviors;

namespace SOTFEdit.Infrastructure;

public class ItemDoubleClickBehavior : Behavior<ListBox>
{
    #region Properties

    private MouseButtonEventHandler? _handler;

    #endregion

    #region Methods

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseDoubleClick += _handler = (_, e) =>
        {
            e.Handled = true;
            if (e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            var sourceItem = source as ListBoxItem ?? source.TryFindParent<ListBoxItem>();

            if (sourceItem == null)
            {
                return;
            }

            foreach (var binding in AssociatedObject.InputBindings.OfType<MouseBinding>())
            {
                if (binding.MouseAction != MouseAction.LeftDoubleClick)
                {
                    continue;
                }

                var command = binding.Command;
                var parameter = binding.CommandParameter;

                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }
        };
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewMouseDoubleClick -= _handler;
    }

    #endregion
}