using System;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace SOTFEdit.Model.Events;

public class ShowDialogEvent(Func<MetroWindow, BaseMetroDialog> dialogFactory)
{
    public Func<MetroWindow, BaseMetroDialog> DialogFactory { get; } = dialogFactory;
}