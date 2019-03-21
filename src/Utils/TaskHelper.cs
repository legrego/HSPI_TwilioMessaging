using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hspi.Utils
{
    internal static class TaskHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "task")]
        public static void StartAsync(Func<Task> taskAction, CancellationToken token)
        {
            var task = Task.Factory.StartNew(() => taskAction(), token,
                                          TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
                                          TaskScheduler.Current);
        }
    }
}