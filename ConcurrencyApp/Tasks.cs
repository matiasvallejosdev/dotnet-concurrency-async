
public static class Tasks
{

    public static Task Delay(int miliseconds)
    {
        // It will return a task that will be completed after 1 second
        // Thread will not be blocked until the task is completed
        return Task.Delay(miliseconds);
    }
}