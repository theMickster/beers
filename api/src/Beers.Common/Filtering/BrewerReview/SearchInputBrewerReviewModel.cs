using Beers.Common.Filtering.Base;

namespace Beers.Common.Filtering.BrewerReview;

public sealed class SearchInputBrewerReviewModel : SearchInputModelBase
{
    public Guid? BrewerId { get; set; }

    public string ReviewerName { get; set; } = string.Empty;

    public int? MinimumRating { get; set; }

    public int? MaximumRating { get; set; }
}
