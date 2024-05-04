using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Constants;
using Beers.Domain.Models.Beer;
using FluentValidation;

namespace Beers.Application.Validators.Beer;

public sealed class CreateBeerValidator : BaseBeerValidator<CreateBeerModel>
{
    private readonly IReadBeerTypeService _readBeerTypeService;
    private readonly IReadBeerCategoryService _readBeerCategoryService;
    private readonly IReadBeerStyleService _readBeerStyleService;

    public CreateBeerValidator(
        IReadBeerService readBeerService,
        IReadBrewerService readBrewerService,
        IReadBeerCategoryService readBeerCategoryService,
        IReadBeerStyleService readBeerStyleService,
        IReadBeerTypeService readBeerTypeService) : base(readBeerService, readBrewerService)
    {
        _readBeerTypeService = readBeerTypeService;
        _readBeerCategoryService = readBeerCategoryService;
        _readBeerStyleService = readBeerStyleService;

        RuleFor(beer => beer.BeerTypeId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BeerTypeIsNull)
            .WithErrorCode("Rule-01");

        RuleFor(beer => beer)
            .Must( (beer) => BeerTypeExistsAsync(beer.BeerTypeId))
            .When(x => x?.BeerTypeId != null)
            .WithMessage(ValidatorConstants.BeerTypeMustExist)
            .WithErrorCode("Rule-02")
            .OverridePropertyName("BeerTypeId");
        

    }

    private bool BeerTypeExistsAsync(Guid id)
    {
        return _readBeerTypeService.GetList().Any(x => x.Id == id);
    }
}