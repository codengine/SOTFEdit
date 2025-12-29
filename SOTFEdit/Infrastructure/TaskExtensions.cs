using System;
using System.Threading.Tasks;

namespace SOTFEdit.Infrastructure;

public static class TaskExtensions
{
    /// <summary>
    ///     Fire-and-forget helper that observes faults to avoid UnobservedTaskException.
    /// </summary>
    public static void Forget(this Task task, Action<AggregateException>? onFault = null)
    {
        _ = task.ContinueWith(t =>
            {
                var exception = t.Exception;
                if (exception != null)
                {
                    onFault?.Invoke(exception);
                }
            },
            TaskContinuationOptions.OnlyOnFaulted |
            TaskContinuationOptions.ExecuteSynchronously);
    }
}