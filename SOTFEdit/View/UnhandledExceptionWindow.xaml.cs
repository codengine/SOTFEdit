using System;
using System.Windows;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class UnhandledExceptionWindow
{
    public UnhandledExceptionWindow(Window? owner, Exception exception)
    {
        Owner = owner;
        DataContext = new UnhandledExceptionViewModel(exception);
        InitializeComponent();
    }
}