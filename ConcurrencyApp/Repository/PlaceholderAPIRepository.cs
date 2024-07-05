using System.Text.Json;
using ConcurrencyApp.Async;
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

    public async Task<List<Post>> GetPosts()
    {
        using var response = await _client.GetAsync("posts").RetryAsync();
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