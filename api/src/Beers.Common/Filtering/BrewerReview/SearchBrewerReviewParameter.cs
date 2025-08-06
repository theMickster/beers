using Beers.Common.Constants;
using Beers.Common.Filtering.Base;

namespace Beers.Common.Filtering.BrewerReview;

/// <summary>
/// Used to support paging while searching for Brewer Review records.
/// </summary>
public sealed class SearchBrewerReviewParameter : SearchParameterBase
{
    private const string ReviewIdField = "reviewId";
    private const string BrewerIdField = "brewerId";
    private const string ReviewerNameField = "reviewerName";
    private const string RatingField = "rating";
    private string _orderBy = ReviewIdField;

    public string OrderBy
    {
        get
        {
            return _orderBy switch
            {
                BrewerIdField => SortedResultConstants.BrewerId,
                ReviewerNameField => SortedResultConstants.Name,
                RatingField => "Rating",
                _ => "ReviewId"
            };
        }
        set =>
            _orderBy = value.Trim().Equals(ReviewIdField, StringComparison.CurrentCultureIgnoreCase)
                ? ReviewIdField
                : value.Trim().Equals(BrewerIdField, StringComparison.CurrentCultureIgnoreCase)
                    ? BrewerIdField
                    : value.Trim().Equals(ReviewerNameField, StringComparison.CurrentCultureIgnoreCase)
                        ? ReviewerNameField
                        : value.Trim().Equals(RatingField, StringComparison.CurrentCultureIgnoreCase)
                            ? RatingField
                            : ReviewIdField;
    }
}
