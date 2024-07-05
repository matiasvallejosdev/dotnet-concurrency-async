using System.Diagnostics;
using ConcurrencyApp.Repository;
using ConcurrencyApp.Services;

// Inject repository and service interfaces
var apiRepository = new PlaceholderAPIRepository();
var postService = new PostService(apiRepository);

// CancellationToken to cancel the operation after a specified time
var cancellationTokenSource = new CancellationTokenSource();
cancellationTokenSource.CancelAfter(5000); // Cancel after 5 seconds
var cancellationToken = cancellationTokenSource.Token;

Console.WriteLine("Hello, World! Your app is running in a container!");
Console.WriteLine("Delaying for 1 second...");
await Task.Delay(1000); // Correct method name for delay

try
{
    Console.WriteLine("Fetching posts and comments...");

    // Reporting progress with IProgress interface
    IProgress<int> reportProgress = new Progress<int>(percent =>
    {
        Console.WriteLine($"Progress: {percent}%");
    });

    var totalStopwatch = Stopwatch.StartNew(); // Start measuring total time

    // Measure time to fetch posts
    var fetchPostsStopwatch = Stopwatch.StartNew();
    var posts = await postService.GetPosts(); // Asynchronous API call to fetch posts with cancellation support
    fetchPostsStopwatch.Stop();
    Console.WriteLine($"Posts: {posts.Count}");

    if (posts == null || !posts.Any())
    {
        throw new SomethingWentWrongException("Posts not found");
    }
    
    // Concurrency: Limiting the number of concurrent tasks using SemaphoreSlim
    var semaphore = new SemaphoreSlim(20); // Limit to 3 concurrent requests

    // Stopwatch to measure time taken to fetch comments
    var fetchCommentsStopwatch = Stopwatch.StartNew();
    var completedTasks = 0;

    // Parallelism: Creating a collection of tasks to fetch comments for each post concurrently
    var getCommentsTasks = posts.Select(async post =>
    {
        // Limiting concurrency with SemaphoreSlim
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var comments = await postService.GetPostCommentsById(post.id); // Asynchronous API call to fetch comments with cancellation support
            Interlocked.Increment(ref completedTasks); // Thread-safe increment of completed tasks count
            reportProgress.Report((int)((float)completedTasks / posts.Count * 100)); // Reporting progress
            return comments; // Returning comments for the current post
        }
        finally
        {
            semaphore.Release(); // Releasing semaphore slot
        }
    }).ToList();

    var commentsLists = new List<List<Comment>>();

    try
    {
        // Using Task.WhenAny() to process tasks as they complete
        while (getCommentsTasks.Any())
        {
            var finishedTask = await Task.WhenAny(getCommentsTasks); // Awaiting any task to complete
            getCommentsTasks.Remove(finishedTask); // Removing completed task from the list
            try
            {
                commentsLists.Add(await finishedTask); // Adding result of completed task to comments list
            }
            catch (OperationCanceledException)
            {
                // Handling task cancellation silently inside the loop
                break; // Exit the loop if cancellation is requested
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching comments: {ex.Message}");
                // Optionally handle specific task exceptions here
            }
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Operation was canceled."); // Handling task cancellation
    }

    fetchCommentsStopwatch.Stop(); // Stopping stopwatch after all tasks complete

    // Example: Get comments for the first post and print them
    var postId = posts.FirstOrDefault()?.id ?? 1;
    var postComments = commentsLists.FirstOrDefault(comments => comments.Any(c => c.postId == postId)) ?? throw new SomethingWentWrongException($"Comments for post {postId} not found");
    Console.WriteLine($"Comments: {commentsLists.Count}");
    totalStopwatch.Stop(); // Stop measuring total time

    // Output detailed timings
    Console.WriteLine($"Time taken to fetch posts: {fetchPostsStopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine($"Time taken to fetch comments: {fetchCommentsStopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine($"Total time taken to execute the code: {totalStopwatch.ElapsedMilliseconds} ms");
}
catch (OperationCanceledException)
{
    Console.WriteLine("The operation was canceled.");
}
catch (SomethingWentWrongException ex)
{
    Console.WriteLine($"Specific error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"General error: {ex.Message}");
}
