using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Filtering.BrewerReview;
using Beers.Domain.Models.Brewer;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.Brewer;

[ServiceLifetimeScoped]
public sealed class ReadBrewerReviewService(
    IMapper mapper,
    IDbContextFactory<BeersDbContext> dbContextFactory)
    : IReadBrewerReviewService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory =
        dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    public async Task<IReadOnlyList<ReadBrewerReviewModel>> GetListAsync(Guid brewerId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entities = await context.BrewerReviewEntities
            .Where(x => x.BrewerId == brewerId)
            .ToListAsync();
        return _mapper.Map<List<ReadBrewerReviewModel>>(entities).AsReadOnly();
    }

    public async Task<ReadBrewerReviewModel?> GetByIdAsync(Guid brewerId, Guid reviewId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entity = await context.BrewerReviewEntities
            .FirstOrDefaultAsync(x => x.BrewerId == brewerId && x.Id == reviewId);
        return entity == null ? null : _mapper.Map<ReadBrewerReviewModel>(entity);
    }

    public async Task<SearchResultBrewerReviewModel> SearchAsync(
        SearchBrewerReviewParameter parameters,
        SearchInputBrewerReviewModel searchModel)
    {
        if (parameters == null || searchModel == null)
        {
            return new SearchResultBrewerReviewModel
            {
                PageNumber = 1,
                PageSize = 10,
                TotalRecords = 0,
                Results = []
            };
        }

        var result = new SearchResultBrewerReviewModel
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = 0,
            Results = []
        };

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var query = context.BrewerReviewEntities.AsQueryable();

        if (searchModel.BrewerId != null)
        {
            query = query.Where(x => x.BrewerId == searchModel.BrewerId);
        }

        if (searchModel.Id != null)
        {
            query = query.Where(x => x.Id == searchModel.Id);
        }

        if (!string.IsNullOrWhiteSpace(searchModel.Name))
        {
            query = query.Where(x => x.Title.Contains(searchModel.Name));
        }

        if (!string.IsNullOrWhiteSpace(searchModel.ReviewerName))
        {
            query = query.Where(x => x.ReviewerName.Contains(searchModel.ReviewerName));
        }

        if (searchModel.MinimumRating.HasValue)
        {
            query = query.Where(x => x.Rating >= searchModel.MinimumRating.Value);
        }

        if (searchModel.MaximumRating.HasValue)
        {
            query = query.Where(x => x.Rating <= searchModel.MaximumRating.Value);
        }

        var totalRecords = await query.CountAsync();

        query = parameters.OrderBy switch
        {
            "BrewerId" => query.OrderBy(x => x.BrewerId),
            "Name" => query.OrderBy(x => x.ReviewerName),
            "Rating" => query.OrderBy(x => x.Rating),
            _ => query.OrderBy(x => x.Id)
        };

        if (parameters.SortOrder == SortedResultConstants.Descending)
        {
            query = parameters.OrderBy switch
            {
                "BrewerId" => query.OrderByDescending(x => x.BrewerId),
                "Name" => query.OrderByDescending(x => x.ReviewerName),
                "Rating" => query.OrderByDescending(x => x.Rating),
                _ => query.OrderByDescending(x => x.Id)
            };
        }

        query = query.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);
        var entities = await query.ToListAsync();
        result.TotalRecords = totalRecords;
        result.Results = _mapper.Map<List<ReadBrewerReviewModel>>(entities);
        return result;
    }
}
