using System.Diagnostics;

Console.WriteLine("Hello, World! Your app is running in a container!");
await Task.Delay(1000); // Correct method name for delay

try
{
    // Reporting progress with IProgress interface
    IProgress<int> reportProgress = new Progress<int>(percent =>
    {
        Console.WriteLine($"Progress: {percent}%");
    });

    var totalStopwatch = Stopwatch.StartNew(); // Start measuring total time
    var api = new JsonPlaceholderApi();

    // Measure time to fetch posts
    var fetchPostsStopwatch = Stopwatch.StartNew();
    var posts = await api.GetPosts();
    fetchPostsStopwatch.Stop();

    if (posts == null || !posts.Any())
    {
        throw new SomethingWentWrongException("Posts not found");
    }

    // Example: Get the first post and print its title
    var postId = posts[0].id;
    var post = posts.FirstOrDefault(p => p.id == postId);
    Console.WriteLine($"First post: {post?.title}");

    // Concurrency: Limiting the number of concurrent tasks using SemaphoreSlim
    var semaphore = new SemaphoreSlim(3); // Limit to 3 concurrent requests

    // Stopwatch to measure time taken to fetch comments
    var fetchCommentsStopwatch = Stopwatch.StartNew();
    var completedTasks = 0;

    // Parallelism: Creating a collection of tasks to fetch comments for each post concurrently
    var getCommentsTasks = posts.Select(async post =>
    {
        // Limiting concurrency with SemaphoreSlim
        await semaphore.WaitAsync();
        try
        {
            var comments = await api.GetComments(post.id); // Asynchronous API call to fetch comments
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

    // Using Task.WhenAny() to process tasks as they complete
    while (getCommentsTasks.Any())
    {
        var finishedTask = await Task.WhenAny(getCommentsTasks); // Awaiting any task to complete
        getCommentsTasks.Remove(finishedTask); // Removing completed task from the list

        try
        {
            commentsLists.Add(await finishedTask); // Adding result of completed task to comments list
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching comments: {ex.Message}");
            // Optionally handle specific task exceptions here
        }
    }

    fetchCommentsStopwatch.Stop(); // Stopping stopwatch after all tasks complete

    // Example: Get comments for the first post and print them
    var postComments = commentsLists.FirstOrDefault(comments => comments.Any(c => c.postId == postId)) ?? throw new SomethingWentWrongException($"Comments for post {postId} not found");
    Console.WriteLine($"Comments: {commentsLists.Count}");
    totalStopwatch.Stop(); // Stop measuring total time

    // Output detailed timings
    Console.WriteLine($"Time taken to fetch posts: {fetchPostsStopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine($"Time taken to fetch comments: {fetchCommentsStopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine($"Total time taken to execute the code: {totalStopwatch.ElapsedMilliseconds} ms");
}
catch (SomethingWentWrongException ex)
{
    Console.WriteLine($"Specific error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"General error: {ex.Message}");
}
