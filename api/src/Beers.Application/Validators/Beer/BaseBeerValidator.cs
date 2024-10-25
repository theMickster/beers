using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Application.Services;
using Beers.Common.Constants;
using Beers.Domain.Models.Base;
using Beers.Domain.Models.Metadata;
using FluentValidation;

namespace Beers.Application.Validators.Beer;

public abstract class BaseBeerValidator<T> : AbstractValidator<T> where T : BaseBeerModel
{
    protected readonly IReadBrewerService ReadBrewerService;
    protected readonly IReadBeerService ReadBeerService;
    protected readonly IReadBeerTypeService ReadBeerTypeService;
    protected readonly IReadBeerCategoryService ReadBeerCategoryService;
    protected readonly IReadBeerStyleService ReadBeerStyleService;

    protected BaseBeerValidator(
        IReadBeerService readBeerService, 
        IReadBrewerService readBrewerService, 
        IReadBeerCategoryService readBeerCategoryService,
        IReadBeerStyleService readBeerStyleService,
        IReadBeerTypeService readBeerTypeService)
    {
        ReadBeerService = readBeerService;
        ReadBrewerService = readBrewerService;
        ReadBeerTypeService = readBeerTypeService;
        ReadBeerCategoryService = readBeerCategoryService;
        ReadBeerStyleService = readBeerStyleService;

        RuleFor(beer => beer)
            .MustAsync(async (beer, cancellation)
                => await BrewerExistsAsync(beer.BrewerId).ConfigureAwait(false))
            .When(x => x?.BrewerId != null)
            .WithMessage(ValidatorConstants.BrewerMustExist)
            .WithErrorCode("Rule-01")
            .OverridePropertyName("BrewerId");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("Rule-02").WithMessage(ValidatorConstants.MessageNameEmpty)
            .MaximumLength(256)
            .WithErrorCode("Rule-03").WithMessage(ValidatorConstants.MessageNameLength);

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithErrorCode("Rule-04").WithMessage(ValidatorConstants.MessageDescriptionEmpty)
            .MaximumLength(256)
            .WithErrorCode("Rule-05").WithMessage(ValidatorConstants.MessageDescriptionLength);
    }

    protected async Task<bool> BrewerExistsAsync(Guid brewerId)
    {
        var result = await ReadBrewerService.GetByIdAsync(brewerId);
        return result != null && result.BrewerId != Guid.Empty;
    }

    protected async Task<bool> BeerTypeExistsAsync(Guid id)
    {
        var result = await ReadBeerTypeService.GetListAsync<BeerTypeModel>();
        return result.Any(x => x.Id == id);
    }
}
