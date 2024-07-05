using ConcurrencyApp.Filters;

namespace ConcurrencyApp.Repository;

public interface IPlaceholderAPIRepository
{
    public Task<List<Post>> GetPosts(PostFilter? postFilter);
    public Task<List<Comment>> GetComments();
    public Task<List<Comment>> GetComment(int postId);
}