using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Constants;
using Beers.Domain.Models.Beer;
using FluentValidation;

namespace Beers.Application.Validators.Beer;

public sealed class UpdateBeerValidator : CreateBeerValidator<UpdateBeerModel>
{
    public UpdateBeerValidator(
        IReadBeerService readBeerService,
        IReadBrewerService readBrewerService,
        IReadBeerCategoryService readBeerCategoryService,
        IReadBeerStyleService readBeerStyleService,
        IReadBeerTypeService readBeerTypeService)
        : base(readBeerService, readBrewerService, readBeerCategoryService, readBeerStyleService, readBeerTypeService)
    {
        RuleFor(beer => beer.BeerId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerIdIsNull)
            .WithErrorCode("Rule-01");
    }
}
