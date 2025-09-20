using Beers.Domain.Models.NewsBlogPost;
using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.NewsBlogPost;

public interface ICreateNewsBlogPostService
{
    /// <summary>
    /// Creates a new News/Blog post for a brewer.
    /// </summary>
    Task<(ReadNewsBlogPostModel, List<ValidationFailure>)> CreateAsync(CreateNewsBlogPostModel inputModel);
}
