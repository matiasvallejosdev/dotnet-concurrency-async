using System.Text.Json;
using ConcurrencyApp.Async;
using ConcurrencyApp.Filters;
using ConcurrencyApp.Models;

namespace ConcurrencyApp.Repository;



public class PlaceholderAPIRepository : IPlaceholderAPIRepository
{
    private const string BaseUrl = "https://jsonplaceholder.typicode.com/";
    private const int _delay = 800;
    private protected readonly HttpClient _client;

    public PlaceholderAPIRepository()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public async Task<List<Post>> GetPosts(PostFilter? postFilter = null)
    {
        var query = "?";
        if (postFilter!.userId.HasValue)
        {
            query += $"userId={postFilter.userId}";
        }
        if (!string.IsNullOrWhiteSpace(postFilter.title))
        {
            query += string.IsNullOrWhiteSpace(query) ? $"title={postFilter.title}" : $"&title={postFilter.title}";
        }
        if (!string.IsNullOrWhiteSpace(postFilter.body))
        {
            query += string.IsNullOrWhiteSpace(query) ? $"body={postFilter.body}" : $"&body={postFilter.body}";
        }
        Console.WriteLine($"Query: {query}");
        using var response = await _client.GetAsync($"posts{query}").TimeExecutionAsync().DelayExecution(_delay);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Post>>(content) ?? new List<Post>();
    }

    public async Task<List<Comment>> GetComments()
    {
        using var response = await _client.GetAsync("comments").TimeoutAfter(1000);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Comment>>(content) ?? new List<Comment>();
    }

    public async Task<List<Comment>> GetComment(int postId)
    {
        using var response = await _client.GetAsync($"comments?postId={postId}").TimeExecutionAsync().DelayExecution(_delay);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Comment>>(content) ?? new List<Comment>();
    }

    public async Task<List<User>> GetUsers()
    {
        var cts = new CancellationTokenSource(1000); // Set timeout for 1000 milliseconds (1 second)
        using var response = await _client.GetAsync("users").RetryAsync().WithCancellation(cts.Token);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<User>>(content) ?? new List<User>();
    }
}