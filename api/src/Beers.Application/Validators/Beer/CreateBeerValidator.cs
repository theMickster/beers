using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Constants;
using Beers.Domain.Models.Beer;
using FluentValidation;

namespace Beers.Application.Validators.Beer;

public class CreateBeerValidator<T> : BaseBeerValidator<T> where T : CreateBeerModel
{
    public CreateBeerValidator(
        IReadBeerService readBeerService,
        IReadBrewerService readBrewerService,
        IReadBeerCategoryService readBeerCategoryService,
        IReadBeerStyleService readBeerStyleService,
        IReadBeerTypeService readBeerTypeService) 
        : base(readBeerService, readBrewerService, readBeerCategoryService, readBeerStyleService, readBeerTypeService)
    {

        RuleFor(beer => beer.BeerTypeId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerTypeIsNull)
            .WithErrorCode("Rule-01");

        RuleFor(beer => beer)
            .MustAsync( async (beer, cancellation) => await BeerTypeExistsAsync(beer.BeerTypeId).ConfigureAwait(false))
            .When(x => x?.BeerTypeId != null)
            .WithMessage(ValidatorConstants.BeerTypeMustExist)
            .WithErrorCode("Rule-02")
            .OverridePropertyName("BeerTypeId");
        

    }


}