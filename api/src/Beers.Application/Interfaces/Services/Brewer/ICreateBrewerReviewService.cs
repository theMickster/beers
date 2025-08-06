using Beers.Domain.Models.Brewer;
using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.Brewer;

public interface ICreateBrewerReviewService
{
    Task<(ReadBrewerReviewModel, List<ValidationFailure>)> CreateAsync(CreateBrewerReviewModel inputModel);
}
