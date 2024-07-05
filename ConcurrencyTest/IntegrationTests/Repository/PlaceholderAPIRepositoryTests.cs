
using ConcurrencyApp.Filters;
using ConcurrencyApp.Repository;

namespace ConcurrencyTest.IntegrationTests.Repository;

public class PlaceholderAPIRepositoryTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetPosts_Filtered_Valid(int userId)
    {
        // Arrange
        var repository = new PlaceholderAPIRepository();
        var postFilter = new PostFilter { userId = userId };

        // Act
        var posts = await repository.GetPosts(postFilter);
        var firstPost = posts.FirstOrDefault();

        // Assert
        Assert.NotNull(posts);
        Assert.Equal(userId, firstPost?.userId);
    }

    [Fact]
    public async Task GetPosts_Filtered_Invalid()
    {
        // Arrange
        var repository = new PlaceholderAPIRepository();
        var postFilter = new PostFilter { userId = 0 };

        // Act
        var posts = await repository.GetPosts(postFilter);

        // Assert
        Assert.NotNull(posts);
        Assert.Empty(posts);
    }

    [Fact]
    public async Task GetComments_Valid()
    {
        // Arrange
        var repository = new PlaceholderAPIRepository();

        // Act
        var comments = await repository.GetComments();

        // Assert
        Assert.NotNull(comments);
        Assert.NotEmpty(comments);
    }
}