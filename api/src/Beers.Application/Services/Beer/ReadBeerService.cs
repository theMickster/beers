using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Filtering.Beer;
using Beers.Domain.Models.Beer;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Beers.Application.Services.Beer;

[ServiceLifetimeScoped]
public sealed class ReadBeerService(IMapper mapper, IDbContextFactory<BeersDbContext> dbContextFactory) 
    : IReadBeerService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    /// <summary>
    /// Retrieve a list of beers
    /// </summary>
    /// <returns>A <see cref="List{T}"/> where T is a <see cref="ReadBeerModel"/></returns>
    public async Task<IReadOnlyList<ReadBeerModel>> GetListAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entities = await context.BeerEntities.ToListAsync();
        return _mapper.Map<List<ReadBeerModel>>(entities).AsReadOnly();
    }


    /// <summary>
    /// Search through the list of beers for desired results
    /// </summary>
    /// <param name="parameters">The search parameters</param>
    /// <param name="searchModel">The search input model</param>
    /// <returns>A detailed result model of type <see cref="ReadBeerModel"/></returns>
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public async Task<SearchResultBeerModel> SearchAsync(SearchBeerParameter parameters, SearchInputBeerModel searchModel)
    {
        var result = new SearchResultBeerModel
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = 0,
            Results = []
        };
        
        if (parameters == null)
        {
            return result;
        }

        if (searchModel == null)
        {
            return result;
        }

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var query = context.BeerEntities.AsQueryable();

        if (searchModel.Id != null)
        {
            query = query.Where(x => x.Id == searchModel.Id);
        }

        if (searchModel.BrewerId != null)
        {
            query = query.Where(x => x.BrewerId == searchModel.BrewerId);
        }

        if (!string.IsNullOrEmpty(searchModel.Name))
        {
            query = query.Where(x => x.Name.Contains(searchModel.Name));
        }
        
        if (!string.IsNullOrEmpty(searchModel.BrewerName))
        {
            query = query.Where(x => x.Brewer.Name.Contains(searchModel.BrewerName));
        }
        
        var totalRecords = await query.CountAsync();
        query = query.OrderBy(x => x.Name);
        if (parameters.SortOrder == SortedResultConstants.Descending)
        {
            query = query.OrderByDescending(x => x.Name);
        }
        
        query = query.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);
        var entities = await query.ToListAsync();
        result.TotalRecords = totalRecords;
        result.Results = _mapper.Map<List<ReadBeerModel>>(entities);
        
        return result;
    }

    /// <summary>
    /// Retrieve a single beer by its unique identifier
    /// </summary>
    /// <param name="beerId">the unique identifier for a given record</param>
    /// <returns>A <see cref="ReadBeerModel"/> when the id matches, otherwise null</returns>
    public async Task<ReadBeerModel?> GetByIdAsync(Guid beerId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entity = await context.BeerEntities.FirstOrDefaultAsync(x => x.Id == beerId);
        return entity == null ? null : _mapper.Map<ReadBeerModel>(entity);
    }
}
