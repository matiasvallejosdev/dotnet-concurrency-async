// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

Console.WriteLine("Hello, World! Your app is running in a container!");
await Task.Delay(1000); // Correct method name for delay

try
{
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

    // Use SemaphoreSlim to limit the number of concurrent tasks
    var semaphore = new SemaphoreSlim(3); // Limit to 3 concurrent requests
    var fetchCommentsStopwatch = Stopwatch.StartNew();
    var completedTasks = 0;
    var getCommentsTasks = posts.Select(async post =>
    {
        await semaphore.WaitAsync();
        try
        {
            var comments = await api.GetComments(post.id);
            Interlocked.Increment(ref completedTasks);
            reportProgress.Report((int)((float)completedTasks / posts.Count * 100));
            return comments;
        }
        finally
        {
            semaphore.Release();
        }
    }).ToList();

    var commentsLists = await Task.WhenAll(getCommentsTasks);
    fetchCommentsStopwatch.Stop();

    // Example: Get comments for the first post and print them
    var postComments = commentsLists.FirstOrDefault(comments => comments.Any(c => c.postId == postId)) ?? throw new SomethingWentWrongException($"Comments for post {postId} not found");
    Console.WriteLine($"Comments: {commentsLists.Length}");
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
    