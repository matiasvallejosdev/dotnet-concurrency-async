
namespace ConcurrencyApp.Async;

public static class AsyncHelper
{
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

    internal static async Task Delay(int delay)
    {
        await Task.Delay(delay);
    }
}