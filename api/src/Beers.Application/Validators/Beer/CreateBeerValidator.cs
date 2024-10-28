using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
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
            .WithErrorCode("Rule-01")
            .OverridePropertyName("BeerStyles");

        RuleFor(beer => beer)
            .MustAsync(async (beer, cancellation) => await BeerStylesExistsAsync(beer.BeerStyles))
            .When(x => x?.BeerStyles is { Count: > 0 })
            .WithMessage(ValidatorConstants.BeerStylesMustExist)
            .WithErrorCode("Rule-04")
            .OverridePropertyName("BeerStyles");
    }

    protected void ValidateBeerCategories()
    {
        RuleFor(x => x.BeerCategories)
            .NotNull()
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerCategoriesIsNullOrEmpty)
            .WithErrorCode("Rule-01")
            .OverridePropertyName("BeerCategories");

        RuleFor(beer => beer)
            .MustAsync(async (beer, cancellation)  => await BeerCategoriesExistsAsync(beer.BeerCategories) )
            .When(x => x?.BeerCategories is { Count: > 0 })
            .WithMessage(ValidatorConstants.BeerCategoriesMustExist)
            .WithErrorCode("Rule-02")
            .OverridePropertyName("BeerCategories");
    }

    protected void ValidateBeerType()
    {
        RuleFor(beer => beer.BeerTypeId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerTypeIsNull)
            .WithErrorCode("Rule-01");

        RuleFor(beer => beer)
            .MustAsync( async (beer, cancellation) => await BeerTypeExistsAsync(beer.BeerTypeId))
            .When(x => x?.BeerTypeId != null)
            .WithMessage(ValidatorConstants.BeerTypeMustExist)
            .WithErrorCode("Rule-02")
            .OverridePropertyName("BeerTypeId");
    }
}