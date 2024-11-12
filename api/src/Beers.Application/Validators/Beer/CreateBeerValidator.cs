using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Common.Constants;
using Beers.Domain.Models.Beer;
using FluentValidation;

namespace Beers.Application.Validators.Beer;

public class CreateBeerValidator : BaseBeerValidator<CreateBeerModel>
{
    public CreateBeerValidator(
        IReadBeerService readBeerService,
        IReadBrewerService readBrewerService,
        IReadBeerCategoryService readBeerCategoryService,
        IReadBeerStyleService readBeerStyleService,
        IReadBeerTypeService readBeerTypeService) 
        : base(readBeerService, readBrewerService, readBeerCategoryService, readBeerStyleService, readBeerTypeService)
    {
        ValidateBrewer();
        ValidateBeerName();
        ValidateBeerNameIsUnique();
        ValidateDescription();
        ValidateBeerType();
        ValidateBeerCategories();
        ValidateBeerStyles();
    }

    protected void ValidateBeerStyles()
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

    protected void ValidateBeerCategories()
    {
        RuleFor(x => x.BeerCategories)
            .NotNull()
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerCategoriesIsNullOrEmpty)
            .OverridePropertyName("BeerCategories");

        RuleFor(beer => beer)
            .MustAsync(async (beer, cancellation)  => await BeerCategoriesExistsAsync(beer.BeerCategories) )
            .When(x => x?.BeerCategories is { Count: > 0 })
            .WithMessage(ValidatorConstants.BeerCategoriesMustExist)
            .OverridePropertyName("BeerCategories");
    }

    protected void ValidateBeerType()
    {
        RuleFor(beer => beer.BeerTypeId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerTypeIsNull);

        RuleFor(beer => beer)
            .MustAsync( async (beer, cancellation) => await BeerTypeExistsAsync(beer.BeerTypeId))
            .When(x => x?.BeerTypeId != null)
            .WithMessage(ValidatorConstants.BeerTypeMustExist)
            .OverridePropertyName("BeerTypeId");
    }

    private void ValidateBeerNameIsUnique()
    {
        RuleFor(x => x)
            .MustAsync(async (x, cancellation) => await BeerNameExistsAsync(x.Name))
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage(ValidatorConstants.BeerNameIsUnique)
            .OverridePropertyName("Name");
    }
}