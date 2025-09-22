using Beers.Domain.Models.NewsBlogPost;
using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.NewsBlogPost;

/// <summary>
/// Updates news/blog posts for brewers.
/// </summary>
public interface IUpdateNewsBlogPostService
{
    /// <summary>
    /// Updates an existing News/Blog post for a brewer.
    /// </summary>
    /// <param name="inputModel">The update model containing post details.</param>
    /// <returns>A tuple containing the updated post and any validation errors.</returns>
    Task<(ReadNewsBlogPostModel, List<ValidationFailure>)> UpdateAsync(UpdateNewsBlogPostModel inputModel);
}
