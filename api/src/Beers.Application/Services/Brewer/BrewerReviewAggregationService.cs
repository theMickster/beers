using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Attributes;
using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.Brewer;

[ServiceLifetimeScoped]
public sealed class BrewerReviewAggregationService(
    IDbContextFactory<BeersDbContext> dbContextFactory)
    : IBrewerReviewAggregationService
{
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory =
        dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    public async Task RecalculateBrewerRatingAsync(Guid brewerId, CancellationToken cancellationToken = default)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var brewerEntity = await context.BrewerEntities
            .FirstOrDefaultAsync(x => x.BrewerId == brewerId, cancellationToken);

        if (brewerEntity == null)
        {
            return;
        }

        var reviewRatings = await context.BrewerReviewEntities
            .Where(x => x.BrewerId == brewerId)
            .Select(x => x.Rating)
            .ToListAsync(cancellationToken);

        brewerEntity.Rating = BuildAggregate(reviewRatings);
        brewerEntity.ModifiedDate = DateTime.UtcNow;

        context.Update(brewerEntity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public static RatingEntity BuildAggregate(IReadOnlyCollection<int> ratings)
    {
        return new RatingEntity
        {
            ReviewCount = ratings.Count,
            Average = ratings.Count == 0 ? 0 : Convert.ToDecimal(ratings.Average())
        };
    }
}
