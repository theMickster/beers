using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.NewsBlogPost;

public interface IDeleteNewsBlogPostService
{
    Task<(bool, List<ValidationFailure>)> DeleteAsync(Guid brewerId, Guid postId);
}
