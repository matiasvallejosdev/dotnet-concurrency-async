using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class JsonPlaceholderApi
{
    private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("https://jsonplaceholder.typicode.com/") };

    public async Task<List<Post>> GetPosts()
    {
        var response = await client.GetAsync("posts");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Post>>(content) ?? [];
    }

    public async Task<List<Comment>> GetComments()
    {
        var response = await client.GetAsync("comments");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Comment>>(content) ?? [];
    }

    public async Task<List<Comment>> GetComments(int postId)
    {
        var response = await client.GetAsync($"comments?postId={postId}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Comment>>(content) ?? [];
    }
}
