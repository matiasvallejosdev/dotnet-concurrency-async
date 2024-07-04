namespace ConcurrencyApp.Services;

public interface IPostService
{
    Task<List<Post>> GetPosts();
    Task<List<Comment>> GetAllComments();
    Task<List<Comment>> GetPostCommentsById(int postId);
}