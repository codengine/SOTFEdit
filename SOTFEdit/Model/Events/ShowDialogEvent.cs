using System;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace SOTFEdit.Model.Events;

public class ShowDialogEvent
{
    public ShowDialogEvent(Func<MetroWindow, BaseMetroDialog> dialogFactory)
    {
        DialogFactory = dialogFactory;
    }

    public Func<MetroWindow, BaseMetroDialog> DialogFactory { get; }
}