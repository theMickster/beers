using Beers.Common.Constants;
using Beers.Common.Filtering.Base;

namespace Beers.Common.Filtering.NewsBlogPost;

public sealed class SearchNewsBlogPostParameter : SearchParameterBase
{
    private const string PostIdField = "postId";
    private const string BrewerIdField = "brewerId";
    private const string TitleField = "title";
    private const string PublishedDateField = "publishedDate";
    private string _orderBy = PostIdField;

    public string OrderBy
    {
        get
        {
            return _orderBy switch
            {
                PostIdField => SortedResultConstants.PostId,
                BrewerIdField => SortedResultConstants.BrewerId,
                TitleField => SortedResultConstants.Title,
                PublishedDateField => SortedResultConstants.PublishedDate,
                _ => SortedResultConstants.PostId
            };
        }
        set =>
            _orderBy = value.Trim().Equals(PostIdField, StringComparison.OrdinalIgnoreCase) ? PostIdField
                : value.Trim().Equals(BrewerIdField, StringComparison.OrdinalIgnoreCase) ? BrewerIdField
                : value.Trim().Equals(TitleField, StringComparison.OrdinalIgnoreCase) ? TitleField
                : value.Trim().Equals(PublishedDateField, StringComparison.OrdinalIgnoreCase) ? PublishedDateField : PostIdField;
    }
}
