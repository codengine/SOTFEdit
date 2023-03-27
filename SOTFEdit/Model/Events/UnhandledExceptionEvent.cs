using System;

namespace SOTFEdit.Model.Events;

public class UnhandledExceptionEvent
{
    public UnhandledExceptionEvent(Exception exception)
    {
        Exception = exception;
    }

    public Exception Exception { get; }
}