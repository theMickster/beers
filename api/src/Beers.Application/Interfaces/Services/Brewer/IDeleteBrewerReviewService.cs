using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.Brewer;

public interface IDeleteBrewerReviewService
{
    Task<(bool, List<ValidationFailure>)> DeleteAsync(Guid brewerId, Guid reviewId);
}
