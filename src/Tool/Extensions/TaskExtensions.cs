using System;
using System.Threading;
using System.Threading.Tasks;

namespace PackProject.Tool.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using var timeoutCts = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCts.Token));
            if (completedTask == task)
            {
                timeoutCts.Cancel();
                return await task; 
            }

            throw new TimeoutException("The operation has timed out.");
        }
    }
}