using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Filtering.Beer;
using FluentValidation;

namespace Beers.Application.Validators.Beer;

[ServiceLifetimeScoped]
public sealed class SearchInputBeerModelValidator : AbstractValidator<SearchInputBeerModel>
{
    private const int MaxIdsPerFilter = 25;

    public SearchInputBeerModelValidator()
    {
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0m)
            .When(x => x.MinPrice.HasValue)
            .WithMessage(ValidatorConstants.SearchBeerPriceNegative);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0m)
            .When(x => x.MaxPrice.HasValue)
            .WithMessage(ValidatorConstants.SearchBeerPriceNegative);

        RuleFor(x => x.MaxPrice)
            .Must((model, maxPrice) => maxPrice >= model.MinPrice)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage(ValidatorConstants.SearchBeerPriceRangeInvalid);

        RuleFor(x => x.BeerStyleIds).Must(HaveAtMostMaxIds).WithMessage(ValidatorConstants.FacetedSearchTooManyIds);
        RuleFor(x => x.BeerCategoryIds).Must(HaveAtMostMaxIds).WithMessage(ValidatorConstants.FacetedSearchTooManyIds);
        RuleFor(x => x.BeerTypeIds).Must(HaveAtMostMaxIds).WithMessage(ValidatorConstants.FacetedSearchTooManyIds);
    }

    private static bool HaveAtMostMaxIds(List<Guid>? ids) => ids == null || ids.Count <= MaxIdsPerFilter;
}
