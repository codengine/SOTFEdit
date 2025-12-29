using System;

namespace SOTFEdit.Model.Events;

public class UnhandledExceptionEvent(Exception exception)
{
    public Exception Exception { get; } = exception;
}