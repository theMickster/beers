using Beers.Domain.Models.NewsBlogPost;

namespace Beers.Application.Interfaces.Services.NewsBlogPost;

/// <summary>
/// Reads news/blog posts for brewers.
/// </summary>
public interface IReadNewsBlogPostService
{
    /// <summary>
    /// Retrieve all news/blog posts for a brewer.
    /// </summary>
    /// <param name="brewerId">The brewer identifier.</param>
    /// <returns>A list of posts.</returns>
    Task<IReadOnlyList<ReadNewsBlogPostModel>> GetListAsync(Guid brewerId);

    /// <summary>
    /// Retrieve a single news/blog post for a brewer.
    /// </summary>
    /// <param name="brewerId">The brewer identifier.</param>
    /// <param name="postId">The post identifier.</param>
    /// <returns>The post when found; otherwise null.</returns>
    Task<ReadNewsBlogPostModel?> GetByIdAsync(Guid brewerId, Guid postId);
}
