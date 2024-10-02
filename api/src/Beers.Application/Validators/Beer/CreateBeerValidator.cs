using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Constants;
using Beers.Domain.Models.Beer;
using Beers.Domain.Models.Metadata;
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
            .MustAsync( async (beer, cancellation) => await BeerTypeExistsAsync(beer.BeerTypeId).ConfigureAwait(false))
            .When(x => x?.BeerTypeId != null)
            .WithMessage(ValidatorConstants.BeerTypeMustExist)
            .WithErrorCode("Rule-02")
            .OverridePropertyName("BeerTypeId");
        

    }

    private async Task<bool> BeerTypeExistsAsync(Guid id)
    {
        var result = await _readBeerTypeService.GetListAsync<BeerTypeModel>();
        return result.Any(x => x.Id == id);
    }
}