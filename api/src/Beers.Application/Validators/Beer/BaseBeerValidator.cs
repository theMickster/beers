using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Interfaces.Services.Metadata;
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
            .WithMessage(ValidatorConstants.MessageDescriptionEmpty)
            .MaximumLength(256)
            .WithMessage(ValidatorConstants.MessageDescriptionLength);
    }

    protected void ValidateBeerName()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidatorConstants.MessageNameEmpty)
            .MaximumLength(256)
            .WithMessage(ValidatorConstants.MessageNameLength);
    }

    protected void ValidateBrewer()
    {
        RuleFor(beer => beer)
            .MustAsync(async (beer, cancellation)
                => await BrewerExistsAsync(beer.BrewerId).ConfigureAwait(false))
            .When(x => x?.BrewerId != null)
            .WithMessage(ValidatorConstants.BrewerMustExist)
            .OverridePropertyName("BrewerId");
    }

    protected async Task<bool> BeerNameExistsAsync(string name)
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
            Name = name.Trim()
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
    private async Task<bool> BrewerExistsAsync(Guid brewerId)
    {
        var result = await ReadBrewerService.GetByIdAsync(brewerId);
        return result != null && result.BrewerId != Guid.Empty;
    }

}
