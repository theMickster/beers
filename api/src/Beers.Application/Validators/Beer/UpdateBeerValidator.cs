using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Common.Constants;
using Beers.Common.Filtering.Beer;
using Beers.Domain.Models.Beer;
using FluentValidation;

namespace Beers.Application.Validators.Beer;

public sealed class UpdateBeerValidator : BaseBeerValidator<UpdateBeerModel>
{
    public UpdateBeerValidator(
        IReadBeerService readBeerService,
        IReadBrewerService readBrewerService,
        IReadBeerCategoryService readBeerCategoryService,
        IReadBeerStyleService readBeerStyleService,
        IReadBeerTypeService readBeerTypeService)
        : base(readBeerService, readBrewerService, readBeerCategoryService, readBeerStyleService, readBeerTypeService)
    {
        ValidateBrewer();
        ValidateBeerId();
        ValidateBeerName();
        ValidateDescription();
        ValidateBeerType();
        ValidateBeerCategories();
        ValidateBeerStyles();
        ValidateBeerNameChangeDoesNotCreateDuplicate();
    }

    private void ValidateBeerId()
    {
        RuleFor(beer => beer.BeerId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerIdIsNull);
    }

    private void ValidateBeerNameChangeDoesNotCreateDuplicate()
    {
        RuleFor(x => x)
            .MustAsync(async (x, cancellation) => await BeerNameUpdateIsUnique(x.BeerId, x.Name))
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage(ValidatorConstants.BeerNameIsUnique)
            .OverridePropertyName("Name");
    }

    private void ValidateBeerStyles()
    {
        RuleFor(x => x.BeerStyles)
            .NotNull()
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerStylesIsNullOrEmpty)
            .OverridePropertyName("BeerStyles");

        RuleFor(beer => beer)
            .MustAsync(async (beer, cancellation) => await BeerStylesExistsAsync(beer.BeerStyles))
            .When(x => x?.BeerStyles is { Count: > 0 })
            .WithMessage(ValidatorConstants.BeerStylesMustExist)
            .OverridePropertyName("BeerStyles");
    }

    private void ValidateBeerCategories()
    {
        RuleFor(x => x.BeerCategories)
            .NotNull()
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerCategoriesIsNullOrEmpty)
            .OverridePropertyName("BeerCategories");

        RuleFor(beer => beer)
            .MustAsync(async (beer, cancellation) => await BeerCategoriesExistsAsync(beer.BeerCategories))
            .When(x => x?.BeerCategories is { Count: > 0 })
            .WithMessage(ValidatorConstants.BeerCategoriesMustExist)
            .OverridePropertyName("BeerCategories");
    }

    private void ValidateBeerType()
    {
        RuleFor(beer => beer.BeerTypeId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerTypeIsNull);

        RuleFor(beer => beer)
            .MustAsync(async (beer, cancellation) => await BeerTypeExistsAsync(beer.BeerTypeId))
            .When(x => x?.BeerTypeId != null)
            .WithMessage(ValidatorConstants.BeerTypeMustExist)
            .OverridePropertyName("BeerTypeId");
    }

    private async Task<bool> BeerNameUpdateIsUnique(Guid beerId, string name)
    {
        var param = new SearchBeerParameter
        {
            OrderBy = SortedResultConstants.Ascending,
            PageNumber = 1,
            PageSize = 50,
            SortOrder = SortedResultConstants.Ascending
        };

        var searchModel = new SearchInputBeerModel
        {
            Name = name
        };

        var beers = (await ReadBeerService.SearchAsync(param, searchModel)).Results;

        switch (beers.Count)
        {
            case 0:
            case 1 when beers[0].BeerId == beerId:
                return true;
            default:
                return false;
        }
    }
}
