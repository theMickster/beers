using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.Brewer;

[ServiceLifetimeScoped]
public sealed class DeleteBrewerReviewService(
    IDbContextFactory<BeersDbContext> dbContextFactory,
    IBrewerReviewAggregationService brewerReviewAggregationService)
    : IDeleteBrewerReviewService
{
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory =
        dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    private readonly IBrewerReviewAggregationService _brewerReviewAggregationService =
        brewerReviewAggregationService ?? throw new ArgumentNullException(nameof(brewerReviewAggregationService));

    public async Task<(bool, List<ValidationFailure>)> DeleteAsync(Guid brewerId, Guid reviewId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entityToDelete = await context.BrewerReviewEntities
            .FirstOrDefaultAsync(x => x.BrewerId == brewerId && x.Id == reviewId);

        if (entityToDelete == null)
        {
            return (false,
            [
                new ValidationFailure
                {
                    PropertyName = "ReviewId",
                    ErrorCode = ValidatorConstants.NotFoundErrorCode,
                    ErrorMessage = $"Unable to find brewer review with id {reviewId}"
                }
            ]);
        }

        if (!entityToDelete.IsDeletable)
        {
            return (false,
            [
                new ValidationFailure
                {
                    PropertyName = "ReviewId",
                    ErrorCode = ValidatorConstants.DeleteEntityNotAllowed,
                    ErrorMessage =
                        $"Unable to delete brewer review with id {reviewId} because it is not deletable. You may, however, attempt to set the record to inactive."
                }
            ]);
        }

        context.Remove(entityToDelete);
        var result = await context.SaveChangesAsync();

        if (result != 1)
        {
            return (false, [new ValidationFailure("ReviewId", "Unable to delete the brewer review entity.")]);
        }

        await _brewerReviewAggregationService.RecalculateBrewerRatingAsync(brewerId);
        return (true, []);
    }
}
