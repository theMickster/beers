namespace Beers.Application.Interfaces.Services.Brewer;

public interface IBrewerReviewAggregationService
{
    Task RecalculateBrewerRatingAsync(Guid brewerId, CancellationToken cancellationToken = default);
}
