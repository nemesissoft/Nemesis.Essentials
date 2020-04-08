using JetBrains.Annotations;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nemesis.Essentials.Runtime
{
    public static class TaskUtils
    {
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout, string timeoutExceptionMessage = null)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return await task;
            }
            else
                throw new TimeoutException($"Timeout of {timeout}: {timeoutExceptionMessage ?? "The operation has timed out."}");
        }

        public static Task WithTimeout(this Task task, long timeoutMilliseconds, string timeoutExceptionMessage = null) =>
             task.WithTimeout(TimeSpan.FromMilliseconds(timeoutMilliseconds), timeoutExceptionMessage);

        public static async Task WithTimeout(this Task task, TimeSpan timeout, string timeoutExceptionMessage = null)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                await task;
            }
            else
                throw new TimeoutException($"Timeout of {timeout}: {timeoutExceptionMessage ?? "The operation has timed out."}");
        }

        public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, long timeoutMilliseconds, string timeoutExceptionMessage = null) =>
             task.WithTimeout(TimeSpan.FromMilliseconds(timeoutMilliseconds), timeoutExceptionMessage);


        public static async Task<TResult> TimeoutAfterWithResult<TResult>(this Task<TResult> task, TimeSpan timeout, Func<TResult> timeoutResultGetter)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return await task;
            }
            else
                return timeoutResultGetter();
        }

        public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout, TResult result = default)
            => task.TimeoutAfterWithResult(timeout, () => result);

        public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, long timeoutMilliseconds, TResult result = default)
            => task.TimeoutAfterWithResult(TimeSpan.FromMilliseconds(timeoutMilliseconds), () => result);

        public static IAsyncStateMachine GetStateMachine(Action continuation)
        {
            var target = continuation.Target;
            var field = target.GetType().GetField("m_stateMachine", BindingFlags.NonPublic | BindingFlags.Instance);
            return (IAsyncStateMachine)field?.GetValue(target);
        }

        public static Task Then<T>([NotNull] this Task<T> task, [NotNull] Action<T> action)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (action == null) throw new ArgumentNullException(nameof(action));

            return task.ContinueWith(t => action(t.Result),
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
