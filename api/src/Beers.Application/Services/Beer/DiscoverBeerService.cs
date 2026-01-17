using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Filtering.Beer;
using Beers.Common.Settings;
using Beers.Domain.Models.Beer;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beers.Application.Services.Beer;

[ServiceLifetimeScoped]
public sealed class DiscoverBeerService(
    IMapper mapper,
    IDbContextFactory<BeersDbContext> dbContextFactory,
    IValidator<FacetedSearchInputModel> validator,
    CosmosClient cosmosClient,
    CosmosDbConnectionSettings cosmosSettings,
    ILogger<DiscoverBeerService> logger)
    : IDiscoverBeerService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    private readonly IValidator<FacetedSearchInputModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly CosmosClient _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
    private readonly CosmosDbConnectionSettings _cosmosSettings = cosmosSettings ?? throw new ArgumentNullException(nameof(cosmosSettings));
    private readonly ILogger<DiscoverBeerService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Filter beers and return the matching page along with facet counts.
    /// </summary>
    public async Task<(FacetedSearchResultModel, List<ValidationFailure>)> DiscoverAsync(
        DiscoverBeerParameter parameters, FacetedSearchInputModel input)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(input);

        var result = new FacetedSearchResultModel
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = 0,
            Results = []
        };

        var validationResult = await _validator.ValidateAsync(input);
        if (validationResult.Errors.Count != 0)
        {
            return (result, validationResult.Errors);
        }

        var container = _cosmosClient.GetContainer(_cosmosSettings.DatabaseName, CosmosContainerConstants.MainContainer);

        var countTask = CountAsync(input);
        var pageTask = GetPageAsync(parameters, input);
        var styleTask = GetFacetAsync(container, input, DiscoverFacet.Style);
        var categoryTask = GetFacetAsync(container, input, DiscoverFacet.Category);
        var brewerTask = GetFacetAsync(container, input, DiscoverFacet.Brewer);
        var typeTask = GetFacetAsync(container, input, DiscoverFacet.BeerType);

        await Task.WhenAll(countTask, pageTask, styleTask, categoryTask, brewerTask, typeTask);

        result.TotalRecords = await countTask;
        result.Results = await pageTask;
        result.Facets = new BeerFacetsModel
        {
            StyleFacets = await styleTask,
            CategoryFacets = await categoryTask,
            BrewerFacets = await brewerTask,
            BeerTypeFacets = await typeTask
        };

        return (result, []);
    }

    /// <summary>
    /// Orders facet rows by count descending, then name ascending. GROUP BY can't be combined with ORDER BY, so this happens in memory.
    /// </summary>
    internal static List<FacetItemModel> ToFacetItems(IEnumerable<FacetRow> rows) =>
        rows.Select(x => new FacetItemModel { Id = x.Id, Name = x.Name, Count = x.BeerCount })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    // A DbContext isn't thread-safe, so the count and the page each get their own.
    private async Task<int> CountAsync(FacetedSearchInputModel input)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.BeerEntities.ApplyFilters(input).CountAsync();
    }

    private async Task<List<ReadBeerModel>> GetPageAsync(DiscoverBeerParameter parameters, FacetedSearchInputModel input)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entities = await context.BeerEntities
            .ApplyFilters(input)
            .ApplySort(parameters)
            .Skip(parameters.GetRecordsToSkip())
            .Take(parameters.PageSize)
            .ToListAsync();

        return _mapper.Map<List<ReadBeerModel>>(entities);
    }

    private async Task<List<FacetItemModel>> GetFacetAsync(Container container, FacetedSearchInputModel input, DiscoverFacet facet)
    {
        var rows = new List<FacetRow>();
        var requestCharge = 0d;

        using var iterator = container.GetItemQueryIterator<FacetRow>(DiscoverBeerQueryBuilder.Build(input, facet));
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();
            requestCharge += page.RequestCharge;
            rows.AddRange(page);
        }

        _logger.LogInformation("Discover {Facet} facet query consumed {RU} RU", facet, requestCharge);

        return ToFacetItems(rows);
    }
}
