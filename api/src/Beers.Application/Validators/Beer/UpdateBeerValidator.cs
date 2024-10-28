using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Constants;
using Beers.Domain.Models.Beer;
using FluentValidation;

namespace Beers.Application.Validators.Beer;

public sealed class UpdateBeerValidator : AbstractValidator<UpdateBeerModel>
{
    public UpdateBeerValidator(
        IReadBeerService readBeerService,
        IReadBrewerService readBrewerService,
        IReadBeerCategoryService readBeerCategoryService,
        IReadBeerStyleService readBeerStyleService,
        IReadBeerTypeService readBeerTypeService)
    {
        RuleFor(x => x).SetInheritanceValidator(v => 
            v.Add<UpdateBeerModel>(new CreateBeerValidator(readBeerService, readBrewerService, readBeerCategoryService, readBeerStyleService, readBeerTypeService ))
        );

        RuleFor(beer => beer.BeerId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerIdIsNull)
            .WithErrorCode("Rule-01");
    }
}
