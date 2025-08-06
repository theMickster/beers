using Beers.Common.Filtering.BrewerReview;
using Beers.Domain.Models.Brewer;

namespace Beers.Application.Interfaces.Services.Brewer;

public interface IReadBrewerReviewService
{
    Task<IReadOnlyList<ReadBrewerReviewModel>> GetListAsync(Guid brewerId);

    Task<ReadBrewerReviewModel?> GetByIdAsync(Guid brewerId, Guid reviewId);

    Task<SearchResultBrewerReviewModel> SearchAsync(
        SearchBrewerReviewParameter parameters,
        SearchInputBrewerReviewModel searchModel);
}
