using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Filtering.Beer;
using FluentValidation;

namespace Beers.Application.Validators.Beer;

[ServiceLifetimeScoped]
public sealed class FacetedSearchInputModelValidator : AbstractValidator<FacetedSearchInputModel>
{
    private const int MaxIdsPerFilter = 25;
    private const int MaxNameLength = 100;
    private const decimal MinRatingValue = 0m;
    private const decimal MaxRatingValue = 5m;

    public FacetedSearchInputModelValidator()
    {
        RuleFor(x => x.MaxRating)
            .Must((model, maxRating) => maxRating >= model.MinRating)
            .When(x => x.MinRating.HasValue && x.MaxRating.HasValue)
            .WithMessage(ValidatorConstants.FacetedSearchRatingRangeInvalid);

        RuleFor(x => x.MinRating)
            .InclusiveBetween(MinRatingValue, MaxRatingValue)
            .When(x => x.MinRating.HasValue)
            .WithMessage(ValidatorConstants.FacetedSearchRatingOutOfRange);

        RuleFor(x => x.MaxRating)
            .InclusiveBetween(MinRatingValue, MaxRatingValue)
            .When(x => x.MaxRating.HasValue)
            .WithMessage(ValidatorConstants.FacetedSearchRatingOutOfRange);

        RuleFor(x => x.BeerStyleIds).Must(HaveAtMostMaxIds).WithMessage(ValidatorConstants.FacetedSearchTooManyIds);
        RuleFor(x => x.BeerCategoryIds).Must(HaveAtMostMaxIds).WithMessage(ValidatorConstants.FacetedSearchTooManyIds);
        RuleFor(x => x.BrewerIds).Must(HaveAtMostMaxIds).WithMessage(ValidatorConstants.FacetedSearchTooManyIds);
        RuleFor(x => x.BeerTypeIds).Must(HaveAtMostMaxIds).WithMessage(ValidatorConstants.FacetedSearchTooManyIds);

        RuleFor(x => x.Name)
            .MaximumLength(MaxNameLength)
            .WithMessage(ValidatorConstants.FacetedSearchNameLength);
    }

    private static bool HaveAtMostMaxIds(List<Guid>? ids) => ids == null || ids.Count <= MaxIdsPerFilter;
}
