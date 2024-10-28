using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Constants;
using Beers.Common.Filtering.Beer;
using Beers.Domain.Models.Base;
using Beers.Domain.Models.Metadata;
using FluentValidation;

namespace Beers.Application.Validators.Beer;

public class BaseBeerValidator<T>(
    IReadBeerService readBeerService,
    IReadBrewerService readBrewerService,
    IReadBeerCategoryService readBeerCategoryService,
    IReadBeerStyleService readBeerStyleService,
    IReadBeerTypeService readBeerTypeService) : AbstractValidator<T> where T : BaseBeerModel
{
    protected readonly IReadBrewerService ReadBrewerService = readBrewerService;
    protected readonly IReadBeerService ReadBeerService = readBeerService;
    protected readonly IReadBeerTypeService ReadBeerTypeService = readBeerTypeService;
    protected readonly IReadBeerCategoryService ReadBeerCategoryService = readBeerCategoryService;
    protected readonly IReadBeerStyleService ReadBeerStyleService = readBeerStyleService;

    protected void ValidateDescription()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithErrorCode("Rule-04").WithMessage(ValidatorConstants.MessageDescriptionEmpty)
            .MaximumLength(256)
            .WithErrorCode("Rule-05").WithMessage(ValidatorConstants.MessageDescriptionLength);
    }

    protected void ValidateBeerName()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("Rule-02").WithMessage(ValidatorConstants.MessageNameEmpty)
            .MaximumLength(256)
            .WithErrorCode("Rule-03").WithMessage(ValidatorConstants.MessageNameLength);

        RuleFor(x => x.Name)
            .MustAsync(async (name, cancellation) => await BeerExistsAsync(name))
            .WithMessage(ValidatorConstants.BeerNameIsUnique)
            .WithErrorCode("Rule-04")
            .OverridePropertyName("Name");
    }

    protected void ValidateBrewer()
    {
        RuleFor(beer => beer)
            .MustAsync(async (beer, cancellation)
                => await BrewerExistsAsync(beer.BrewerId).ConfigureAwait(false))
            .When(x => x?.BrewerId != null)
            .WithMessage(ValidatorConstants.BrewerMustExist)
            .WithErrorCode("Rule-01")
            .OverridePropertyName("BrewerId");
    }

    private async Task<bool> BrewerExistsAsync(Guid brewerId)
    {
        var result = await ReadBrewerService.GetByIdAsync(brewerId);
        return result != null && result.BrewerId != Guid.Empty;
    }

    private async Task<bool> BeerExistsAsync(string name)
    {
        var param = new SearchBeerParameter()
        {
            OrderBy = SortedResultConstants.Ascending,
            PageNumber = 1,
            PageSize = 50,
            SortOrder = SortedResultConstants.Ascending
        };

        var searchModel = new SearchInputBeerModel()
        {
            Name = name
        };

        var result = await ReadBeerService.SearchAsync(param, searchModel);
        return result.Results.Count == 0;
    }

    protected async Task<bool> BeerTypeExistsAsync(Guid id)
    {
        var result = await ReadBeerTypeService.GetListAsync<BeerTypeModel>();
        return result.Any(x => x.Id == id);
    }

    protected async Task<bool> BeerCategoriesExistsAsync(List<Guid> ids)
    {
        var models = await ReadBeerCategoryService.GetListAsync<BeerCategoryModel>();
        var result = ids.Count(x => models.All(y => y.Id != x));
        return result == 0;
    }

    protected async Task<bool> BeerStylesExistsAsync(List<Guid> ids)
    {
        var models = await ReadBeerStyleService.GetListAsync<BeerStyleModel>();
        var result = ids.Count(x => models.All(y => y.Id != x));
        return result == 0;
    }
}
