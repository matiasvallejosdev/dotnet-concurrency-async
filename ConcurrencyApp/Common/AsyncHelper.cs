
namespace ConcurrencyApp.Async;

/// <summary>
/// Helper class for asynchronous operations.
/// </summary>
public static class AsyncHelper
{
    // Retry Async Pattern
    public static async Task<T> RetryAsync<T>(Func<Task<T>> action, int retryCount = 3, int delay = 1000)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attempt {i + 1} failed: {ex.Message}");
                await Task.Delay(delay);
            }
        }
        throw new Exception("Retry failed."); // Add a return statement to handle the case when the loop completes without returning a value.
    }

    // Delay Async Pattern
    internal static async Task Delay(int delay)
    {
        await Task.Delay(delay);
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

    // One Execution Async Pattern
    public static async Task<T> OneExecutionAsync<T>(List<Func<CancellationToken, Task<T>>> actions)
    {
        var cancelTokenSource = new CancellationTokenSource();
        var cancelToken = cancelTokenSource.Token;
        var tasks = actions.Select(action => action(cancelToken));
        var completedTask = await Task.WhenAny(tasks);
        cancelTokenSource.Cancel();
        return await completedTask;
    }
}