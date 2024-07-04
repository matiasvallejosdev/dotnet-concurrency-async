using System.Text.Json;

namespace ConcurrencyApp.Repository;

public class PlaceholderAPIRepository : IPlaceholderAPIRepository
{
    private const string BaseUrl = "https://jsonplaceholder.typicode.com/";
    private const int _delay = 300;
    private protected readonly HttpClient _client;

    public PlaceholderAPIRepository()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public async Task<List<Post>> GetPosts()
    {
        var response = await _client.GetAsync("posts");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Post>>(content) ?? [];
    }

    public async Task<List<Comment>> GetComments()
    {
        var response = await _client.GetAsync("comments");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Comment>>(content) ?? [];
    }

    public async Task<List<Comment>> GetComment(int postId)
    {
        await Task.Delay(_delay); // Simulate a delay in the API call
        var response = await _client.GetAsync($"comments?postId={postId}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Comment>>(content) ?? [];
    }
}
