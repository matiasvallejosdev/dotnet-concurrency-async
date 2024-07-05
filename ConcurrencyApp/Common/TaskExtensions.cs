
using System.Diagnostics;

namespace ConcurrencyApp.Async;

/// <summary>
/// Helper class for asynchronous operations.
/// </summary>
public static class TaskExtensions
{
    // Retry Async Pattern
    public static async Task<T> RetryAsync<T>(this Task<T> action, int retryCount = 3, int delay = 1000)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                return await action;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attempt {i + 1} failed: {ex.Message}");
                if (i == retryCount - 1) throw; // rethrow the last exception if it's the last attempt
                await Task.Delay(delay);
            }
        }

        throw new Exception("Retry failed."); // This should never be reached
    }

    // One Execution Async Pattern
    public static async Task<T> OneExecutionAsync<T>(List<Func<CancellationToken, Task<T>>> actions)
    {
        var cancelTokenSource = new CancellationTokenSource();
        var cancelToken = cancelTokenSource.Token;

        // Start all tasks
        var tasks = actions.Select(action => action(cancelToken)).ToList();

        try
        {
            // Await the first task to complete
            var completedTask = await Task.WhenAny(tasks);

            // Cancel the remaining tasks
            cancelTokenSource.Cancel();

            // Await the result of the completed task
            return await completedTask;
        }
        catch (Exception ex)
        {
            // Handle exceptions if necessary
            throw new AggregateException("An error occurred while executing the tasks.", ex);
        }
        finally
        {
            // Ensure the cancellation token source is disposed of
            cancelTokenSource.Dispose();
        }
    }

    // Time Execution Async Pattern
    public static async Task<T> TimeExecutionAsync<T>(this Task<T> task)
    {
        var stopwatch = Stopwatch.StartNew();
        T result = await task;
        stopwatch.Stop();
        return result;
    }

    // Delay Async Pattern
    public static async Task<T> DelayExecution<T>(this Task<T> task, int delay)
    {
        await Task.Delay(delay);
        return await task;
    }

    // Timeout Async Pattern
    public static async Task<T> TimeoutAfter<T>(this Task<T> task, int timeout)
    {
        using (var timeoutCancellationTokenSource = new CancellationTokenSource())
        {
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return await task;
            }
            throw new TimeoutException("The operation has timed out.");
        }
    }

    // WithCancellation Async Pattern
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        using (cancellationToken.Register(s =>
        {
            if (s is TaskCompletionSource<bool> tcs)
            {
                tcs.TrySetResult(true);
            }
        }, tcs))
        {
            if (task != await Task.WhenAny(task, tcs.Task))
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
        return await task;
    }
}