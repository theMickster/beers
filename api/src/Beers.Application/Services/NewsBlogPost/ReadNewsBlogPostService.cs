using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Filtering.NewsBlogPost;
using Beers.Domain.Enums;
using Beers.Domain.Models.NewsBlogPost;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Beers.Application.Services.NewsBlogPost;

[ServiceLifetimeScoped]
public sealed class ReadNewsBlogPostService(IMapper mapper, IDbContextFactory<BeersDbContext> dbContextFactory)
    : IReadNewsBlogPostService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    /// <summary>
    /// Retrieve all news/blog posts for a brewer.
    /// </summary>
    public async Task<IReadOnlyList<ReadNewsBlogPostModel>> GetListAsync(Guid brewerId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

        var entities = await context.NewsBlogPostEntities
            .Where(x => x.BrewerId == brewerId)
            .OrderByDescending(x => x.PublishedDate)
            .ToListAsync()
            .ConfigureAwait(false);

        return _mapper.Map<List<ReadNewsBlogPostModel>>(entities).AsReadOnly();
    }

    /// <summary>
    /// Search news/blog posts with filtering.
    /// </summary>
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public async Task<SearchResultNewsBlogPostModel> SearchAsync(SearchNewsBlogPostParameter parameters, SearchInputNewsBlogPostModel searchModel)
    {
        if (parameters == null)
        {
            return new SearchResultNewsBlogPostModel { PageNumber = 1, PageSize = 10, TotalRecords = 0, Results = [] };
        }

        if (searchModel == null)
        {
            return new SearchResultNewsBlogPostModel { PageNumber = parameters.PageNumber, PageSize = parameters.PageSize, TotalRecords = 0, Results = [] };
        }

        var result = new SearchResultNewsBlogPostModel
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = 0,
            Results = []
        };

        await using var context = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);
        var query = context.NewsBlogPostEntities.AsQueryable();

        if (!searchModel.IncludeDrafts)
        {
            query = query.Where(x => x.PublishedDate != null);
        }

        if (searchModel.Id != null)
        {
            query = query.Where(x => x.Id == searchModel.Id);
        }

        if (searchModel.BrewerId != null)
        {
            query = query.Where(x => x.BrewerId == searchModel.BrewerId);
        }

        if (!string.IsNullOrEmpty(searchModel.PostType) && Enum.TryParse<NewsBlogPostType>(searchModel.PostType, true, out var parsedType))
        {
            query = query.Where(x => x.PostType == parsedType);
        }

        if (!string.IsNullOrEmpty(searchModel.Tag))
        {
            query = query.Where(x => x.Tags.Contains(searchModel.Tag));
        }

        if (searchModel.DateRangeStart.HasValue)
        {
            query = query.Where(x => x.PublishedDate >= searchModel.DateRangeStart);
        }

        if (searchModel.DateRangeEnd.HasValue)
        {
            query = query.Where(x => x.PublishedDate <= searchModel.DateRangeEnd);
        }

        var totalRecords = await query.CountAsync().ConfigureAwait(false);

        query = parameters.OrderBy switch
        {
            var o when o == SortedResultConstants.BrewerId => parameters.SortOrder == SortedResultConstants.Descending
                ? query.OrderByDescending(x => x.BrewerId)
                : query.OrderBy(x => x.BrewerId),
            var o when o == SortedResultConstants.Title => parameters.SortOrder == SortedResultConstants.Descending
                ? query.OrderByDescending(x => x.Title)
                : query.OrderBy(x => x.Title),
            var o when o == SortedResultConstants.PublishedDate => parameters.SortOrder == SortedResultConstants.Descending
                ? query.OrderByDescending(x => x.PublishedDate)
                : query.OrderBy(x => x.PublishedDate),
            _ => parameters.SortOrder == SortedResultConstants.Descending
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id)
        };

        query = query.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);
        var entities = await query.ToListAsync().ConfigureAwait(false);
        result.TotalRecords = totalRecords;
        result.Results = _mapper.Map<List<ReadNewsBlogPostModel>>(entities);

        return result;
    }

    /// <summary>
    /// Retrieve a single news/blog post for a brewer.
    /// </summary>
    public async Task<ReadNewsBlogPostModel?> GetByIdAsync(Guid brewerId, Guid postId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

        var entity = await context.NewsBlogPostEntities
            .FirstOrDefaultAsync(x => x.BrewerId == brewerId && x.Id == postId)
            .ConfigureAwait(false);

        return entity == null ? null : _mapper.Map<ReadNewsBlogPostModel>(entity);
    }
}
