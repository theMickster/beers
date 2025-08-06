using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Constants;
using Beers.Domain.Models.Base;
using FluentValidation;

namespace Beers.Application.Validators.Brewer;

public abstract class BaseBrewerReviewValidator<T>(IReadBrewerService readBrewerService) : AbstractValidator<T>
    where T : BaseBrewerReviewModel
{
    protected readonly IReadBrewerService ReadBrewerService = readBrewerService;

    protected void ValidateBrewer()
    {
        RuleFor(beer => beer.BrewerId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BrewerIdIsNull);

        RuleFor(beer => beer)
            .MustAsync(async (beer, cancellation)
                => await BrewerExistsAsync(beer.BrewerId).ConfigureAwait(false))
            .When(x => x?.BrewerId != null)
            .WithMessage(ValidatorConstants.BrewerMustExist)
            .OverridePropertyName("BrewerId");
    }

    protected void ValidateReviewerName()
    {
        RuleFor(x => x.ReviewerName)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BrewerReviewReviewerNameEmpty)
            .MaximumLength(128)
            .WithMessage(ValidatorConstants.BrewerReviewReviewerNameLength);
    }

    protected void ValidateTitle()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BrewerReviewTitleEmpty)
            .MaximumLength(256)
            .WithMessage(ValidatorConstants.BrewerReviewTitleLength);
    }

    protected void ValidateComments()
    {
        RuleFor(x => x.Comments)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BrewerReviewCommentsEmpty)
            .MaximumLength(4000)
            .WithMessage(ValidatorConstants.BrewerReviewCommentsLength);
    }

    protected void ValidateRating()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage(ValidatorConstants.BrewerReviewRatingOutOfRange);
    }

    private async Task<bool> BrewerExistsAsync(Guid brewerId)
    {
        var result = await ReadBrewerService.GetByIdAsync(brewerId);
        return result != null && result.BrewerId != Guid.Empty;
    }
}
