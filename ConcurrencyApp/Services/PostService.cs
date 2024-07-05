

using ConcurrencyApp.Filters;
using ConcurrencyApp.Repository;

namespace ConcurrencyApp.Services;


public class PostService : IPostService
{
    private readonly IPlaceholderAPIRepository _placeholderAPIRepository;

    public PostService(IPlaceholderAPIRepository placeholderAPIRepository)
    {
        _placeholderAPIRepository = placeholderAPIRepository;
    }

    public async Task<List<Comment>> GetPostCommentsById(int postId)
    {
        var comments = await _placeholderAPIRepository.GetComment(postId);
        return comments;
    }

    public async Task<List<Comment>> GetAllComments()
    {
        var allPostsComments = await _placeholderAPIRepository.GetComments();
        return allPostsComments;
    }

    public async Task<List<Post>> GetPosts()
    {
        var posts = await _placeholderAPIRepository.GetPosts(null);
        return posts;
    }
}